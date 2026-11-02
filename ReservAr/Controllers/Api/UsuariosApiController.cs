using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservAr.Helpers;
using ReservAr.Models.Repositorios;

namespace ReservAr.Controllers.Api;

[ApiController]
[Route("api/usuarios")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.SuperAdmin)]
public class UsuariosApiController : ControllerBase
{
  private const int TamanoPagina = 10;

  private readonly RepositorioUsuario _repoUsuario;

  public UsuariosApiController(RepositorioUsuario repoUsuario)
  {
    _repoUsuario = repoUsuario;
  }

  [HttpGet]
  public IActionResult Listar(string buscar = "", int pagina = 1)
  {
    buscar = (buscar ?? "").Trim();
    if (pagina < 1) pagina = 1;

    var total = _repoUsuario.Contar(buscar);
    var totalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina);
    var items = _repoUsuario.ListarPaginado(buscar, pagina, TamanoPagina)
        .Select(u => new { u.IdUsuario, u.Nombre, u.Apellido, u.Email, u.Telefono, u.Activo, u.FechaAlta });

    return Ok(new { items, total, pagina, totalPaginas });
  }
}