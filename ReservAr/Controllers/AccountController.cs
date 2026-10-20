using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

    public AccountController(RepositorioUsuario repoUsuario, RepositorioRol repoRol)
    {
        _repoUsuario = repoUsuario;
        _repoRol = repoRol;
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
        var rol = _repoRol.ObtenerPorNombre("Usuario");
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