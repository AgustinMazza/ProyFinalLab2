using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservAr.Helpers;
using ReservAr.Models;
using ReservAr.Models.Repositorios;
using ReservAr.ViewModels;

namespace ReservAr.Controllers;

[Authorize(Roles = Roles.SuperAdmin)]
public class UsuariosController : Controller
{
  private const int TamanoPagina = 10; //despues si llego lo hacemos editable, tipo 5 -10 - 15 etc

  private readonly RepositorioUsuario _repoUsuario;
  private readonly RepositorioRol _repoRol;

  public UsuariosController(RepositorioUsuario repoUsuario, RepositorioRol repoRol)
  {
    _repoUsuario = repoUsuario;
    _repoRol = repoRol;
  }

  public IActionResult Index()
  {
    return View();
  }

  [HttpGet]
  public IActionResult Listar(string buscar = "", int pagina = 1)
  {
    buscar = buscar.Trim();
    if (pagina < 1) pagina = 1;

    var total = _repoUsuario.Contar(buscar);
    var totalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina);
    var usuarios = _repoUsuario.ListarPaginado(buscar, pagina, TamanoPagina);
    var roles = _repoRol.Listar();

    var items = usuarios.Select(u => new
    {
      u.IdUsuario,
      u.IdRol,
      Rol = roles.FirstOrDefault(r => r.IdRol == u.IdRol)?.Nombre,
      u.Nombre,
      u.Apellido,
      u.Email,
      u.Telefono,
      u.Activo
    });

    return Json(new { items, total, pagina, totalPaginas });
  }

  [HttpGet]
  public IActionResult ListarRoles()
  {
    return Json(_repoRol.Listar());
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Guardar([FromBody] UsuarioFormViewModel model)//vienen en formato json
  {
    var esAlta = model.IdUsuario == 0;

    if (!esAlta)
      ModelState.Remove(nameof(model.Password));

    if (esAlta && string.IsNullOrWhiteSpace(model.Password))
      ModelState.AddModelError(nameof(model.Password), "Ingresá una contraseña");

    if (!ModelState.IsValid)
    {
      var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
      return BadRequest(new { errores }); //responde 400: la peticion tiene un problema (datos invalidos, email repetido)
    }

    if (_repoRol.ObtenerPorId(model.IdRol) == null)
      return Rechazar("El rol elegido no existe");

    var email = model.Email.Trim();
    var existente = _repoUsuario.ObtenerPorEmail(email);
    if (existente != null && existente.IdUsuario != model.IdUsuario)
      return Rechazar("Ya existe un usuario con ese email");

    var telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim();

    if (esAlta)
    {
      var nuevo = new Usuario
      {
        IdRol = model.IdRol,
        Nombre = model.Nombre.Trim(),
        Apellido = model.Apellido.Trim(),
        Email = email,
        Telefono = telefono,
        PasswordHash = HashHelper.Hashear(model.Password!)
      };
      _repoUsuario.Crear(nuevo);
      return Ok(new { ok = true }); //responde 200: salio bien
    }

    var usuario = _repoUsuario.ObtenerPorId(model.IdUsuario);
    if (usuario == null)
      return NotFound(); //responde 404: lo que pediste no existe

    if (usuario.IdUsuario == IdUsuarioActual() && usuario.IdRol != model.IdRol)
      return Rechazar("No podés cambiarte el rol a vos mismo");

    usuario.IdRol = model.IdRol;
    usuario.Nombre = model.Nombre.Trim();
    usuario.Apellido = model.Apellido.Trim();
    usuario.Email = email;
    usuario.Telefono = telefono;
    _repoUsuario.Modificar(usuario);

    return Ok(new { ok = true });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Baja(int id)
  {
    if (id == IdUsuarioActual())
      return Rechazar("No podés dar de baja tu propia cuenta");

    var filas = _repoUsuario.Eliminar(id);
    return filas > 0 ? Ok(new { ok = true }) : NotFound();
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Reactivar(int id)
  {
    var filas = _repoUsuario.Reactivar(id);
    return filas > 0 ? Ok(new { ok = true }) : NotFound();
  }

  private int IdUsuarioActual() // para controlar la baja
  {
    return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
  }

  private IActionResult Rechazar(string mensaje) //metodo mensaje
  {
    return BadRequest(new { errores = new[] { mensaje } }); //responde 400: la peticion tiene un problema (datos invalidos, email repetido)
  }
}