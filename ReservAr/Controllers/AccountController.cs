using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservAr.Helpers;
using ReservAr.Models;
using ReservAr.Models.Repositorios;
using ReservAr.ViewModels;

namespace ReservAr.Controllers;

public class AccountController : Controller
{
  private readonly RepositorioUsuario _repoUsuario;
  private readonly RepositorioRol _repoRol;
  private readonly IWebHostEnvironment _env;

  public AccountController(RepositorioUsuario repoUsuario, RepositorioRol repoRol, IWebHostEnvironment env)
  {
    _repoUsuario = repoUsuario;
    _repoRol = repoRol;
    _env = env;
  }

  //login
  [HttpGet]
  public IActionResult Login(string? returnUrl = null)
  {
    if (User.Identity?.IsAuthenticated == true)
      return RedirectToAction("Index", "Home");

    return View(new LoginViewModel { ReturnUrl = returnUrl });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Login(LoginViewModel model)
  {
    if (!ModelState.IsValid)
      return View(model);

    var usuario = _repoUsuario.ObtenerPorEmail(model.Email.Trim());


    if (usuario == null || !HashHelper.Verificar(model.Password, usuario.PasswordHash))
    {
      ModelState.AddModelError("", "Email o contraseña incorrectos");
      return View(model);
    }

    if (!usuario.Activo)
    {
      ModelState.AddModelError("", "Tu cuenta está dada de baja");
      return View(model);
    }

    var rol = _repoRol.ObtenerPorId(usuario.IdRol);
    if (rol == null)
    {
      ModelState.AddModelError("", "Tu usuario no tiene un rol válido, contactá al administrador");
      return View(model);
    }

    await IniciarSesionAsync(usuario, rol.Nombre);

    // el "IsLocalUrl" evita que alguien arme un link de login que redirija a otro sitio 
    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
      return LocalRedirect(model.ReturnUrl);

    return RedirectToAction("Index", "Home");
  }

  //logout
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Logout()
  {
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return RedirectToAction("Index", "Home");
  }

  //registro
  [HttpGet]
  public IActionResult Registro()
  {
    if (User.Identity?.IsAuthenticated == true)
      return RedirectToAction("Index", "Home");

    return View(new RegistroViewModel());
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Registro(RegistroViewModel model)
  {
    if (!ModelState.IsValid)
      return View(model);

    var email = model.Email.Trim();

    if (_repoUsuario.ObtenerPorEmail(email) != null)
    {
      ModelState.AddModelError(nameof(model.Email), "Ya existe una cuenta con ese email");
      return View(model);
    }

    //El rol siempre es Usuario osea q el que se registra va a ser usuario siempre a no ser q tenga otro rol asignado a la cuenta antes
    var rol = _repoRol.ObtenerPorNombre(Roles.Usuario);
    if (rol == null)
    {
      ModelState.AddModelError("", "No se pudo completar el registro, intentá más tarde");
      return View(model);
    }

    var usuario = new Usuario
    {
      IdRol = rol.IdRol,
      Nombre = model.Nombre.Trim(),
      Apellido = model.Apellido.Trim(),
      Email = email,
      Telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim(),
      PasswordHash = HashHelper.Hashear(model.Password)
    };

    usuario.IdUsuario = _repoUsuario.Crear(usuario);

    //Esto es para q quede logueado directamente después de registrarse
    await IniciarSesionAsync(usuario, rol.Nombre);
    return RedirectToAction("Index", "Home");
  }

  //Acceso denegado
  [HttpGet]
  public IActionResult AccesoDenegado()
  {
    return View();
  }

  //Perfil
  [Authorize]
  [HttpGet]
  public IActionResult Perfil()
  {
    var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var usuario = _repoUsuario.ObtenerPorId(id);
    if (usuario == null)
    {
      return NotFound();
    }
    return View(usuario);
  }

  //CambiarAvatar
  [Authorize]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> CambiarAvatar(IFormFile? archivo)
  {
    if (archivo == null || archivo.Length == 0)
    {
      TempData["Error"] = "Elegi una imagen";
      return RedirectToAction(nameof(Perfil));
    }

    if (archivo.Length > 2 * 1024 * 1024)
    {
      TempData["Error"] = "La imagen no puede pesar mas de 2 Mb";
      return RedirectToAction(nameof(Perfil));
    }

    var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
    var permitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
    if (!permitidas.Contains(extension))
    {
      TempData["Error"] = "Solo se permiten imagenes en formato JPG, JPEG, PNG, WEBP";
      return RedirectToAction(nameof(Perfil));
    }

    var carpeta = Path.Combine(_env.WebRootPath, "uploads", "avatars");
    Directory.CreateDirectory(carpeta);

    var nombreArchivo = $"{Guid.NewGuid()}{extension}";
    var rutaFisica = Path.Combine(carpeta, nombreArchivo);

    using (var stream = new FileStream(rutaFisica, FileMode.Create))
    {
      await archivo.CopyToAsync(stream);
    }

    var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    _repoUsuario.ActualizarAvatar(id, $"/uploads/avatars/{nombreArchivo}");

    TempData["Mensaje"] = "Avatar Actualizado";
    return RedirectToAction(nameof(Perfil));
  }

  //EditarPerfil
  [Authorize]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel model)
  {
    if (!ModelState.IsValid)
    {
      TempData["Error"] = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
      return RedirectToAction(nameof(Perfil));
    }

    var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var usuario = _repoUsuario.ObtenerPorId(id);
    if (usuario == null)
    {
      return NotFound();
    }

    usuario.Nombre = model.Nombre.Trim();
    usuario.Apellido = model.Apellido.Trim();
    usuario.Telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim();
    _repoUsuario.Modificar(usuario);

    //Se vuelve a iniciar sesión para que el nombre del menú se actualice
    await IniciarSesionAsync(usuario, User.FindFirstValue(ClaimTypes.Role)!);

    TempData["Mensaje"] = "Datos actualizados";
    return RedirectToAction(nameof(Perfil));
  }

  //CambiarPassword
  [Authorize]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult CambiarPassword(CambiarPasswordViewModel model)
  {
    if (!ModelState.IsValid)
    {
      TempData["Error"] = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
      return RedirectToAction(nameof(Perfil));
    }

    var email = User.FindFirstValue(ClaimTypes.Email)!;
    var usuario = _repoUsuario.ObtenerPorEmail(email);
    if (usuario == null)
    {
      return NotFound();
    }

    if (!HashHelper.Verificar(model.ActualPassword, usuario.PasswordHash))
    {
      TempData["Error"] = "La contraseña actual no es correcta";
      return RedirectToAction(nameof(Perfil));
    }

    _repoUsuario.ActualizarPassword(usuario.IdUsuario, HashHelper.Hashear(model.NuevaPassword));

    TempData["Mensaje"] = "Contraseña actualizada";
    return RedirectToAction(nameof(Perfil));
  }

  //private, esto es lo que inica la sesion 
  private async Task IniciarSesionAsync(Usuario usuario, string nombreRol)
  {
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, nombreRol)   // debe coincidir exacto con roles.Nombre
        };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(identity));
  }
}