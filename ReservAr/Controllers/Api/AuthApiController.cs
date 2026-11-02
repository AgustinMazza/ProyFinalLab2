using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ReservAr.Helpers;
using ReservAr.Models.Repositorios;
using ReservAr.ViewModels;

namespace ReservAr.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
  private readonly RepositorioUsuario _repoUsuario;
  private readonly RepositorioRol _repoRol;
  private readonly IConfiguration _config;

  public AuthApiController(RepositorioUsuario repoUsuario, RepositorioRol repoRol, IConfiguration config)
  {
    _repoUsuario = repoUsuario;
    _repoRol = repoRol;
    _config = config;
  }

  [HttpPost("login")]
  [AllowAnonymous]
  public IActionResult Login(LoginApiRequest request)
  {
    var usuario = _repoUsuario.ObtenerPorEmail(request.Email.Trim());

    if (usuario == null || !HashHelper.Verificar(request.Password, usuario.PasswordHash))
      return Unauthorized(new { error = "Email o contraseña incorrectos" });

    if (!usuario.Activo)
      return Unauthorized(new { error = "Tu cuenta está dada de baja" });

    var rol = _repoRol.ObtenerPorId(usuario.IdRol);
    if (rol == null)
      return Unauthorized(new { error = "El usuario no tiene un rol válido" });

    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, rol.Nombre)
        };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["TokenAuthentication:SecretKey"]!));
    var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expira = DateTime.UtcNow.AddHours(8);

    var token = new JwtSecurityToken(
        issuer: _config["TokenAuthentication:Issuer"],
        audience: _config["TokenAuthentication:Audience"],
        claims: claims,
        expires: expira,
        signingCredentials: credenciales);

    return Ok(new
    {
      token = new JwtSecurityTokenHandler().WriteToken(token),
      expira,
      rol = rol.Nombre
    });
  }

  [HttpGet("perfil")]
  [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
  public IActionResult Perfil()
  {
    var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var usuario = _repoUsuario.ObtenerPorId(id);
    if (usuario == null) return NotFound();

    return Ok(new
    {
      usuario.IdUsuario,
      usuario.Nombre,
      usuario.Apellido,
      usuario.Email,
      usuario.Telefono,
      usuario.Avatar,
      Rol = User.FindFirstValue(ClaimTypes.Role)
    });
  }
}