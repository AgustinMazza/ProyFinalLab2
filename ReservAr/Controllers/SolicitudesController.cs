using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservAr.Helpers;
using ReservAr.Models;
using ReservAr.Models.Repositorios;
using ReservAr.ViewModels;

namespace ReservAr.Controllers;

[Authorize(Roles = Roles.Usuario)]
public class SolicitudesController : Controller
{
  private readonly RepositorioNegocio _repoNegocio;
  private readonly RepositorioCategoria _repoCategoria;

  public SolicitudesController(RepositorioNegocio repoNegocio, RepositorioCategoria repoCategoria)
  {
    _repoNegocio = repoNegocio;
    _repoCategoria = repoCategoria;
  }

  [HttpGet]
  public IActionResult Index()
  {
    return View(ArmarModelo(new NegocioFormViewModel()));
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Crear([Bind(Prefix = "Form")] NegocioFormViewModel form)
  {
    if (!ModelState.IsValid)
      return View("Index", ArmarModelo(form));

    if (_repoCategoria.ObtenerPorId(form.IdCategoria) == null)
    {
      ModelState.AddModelError("Form.IdCategoria", "La categoría elegida no existe");
      return View("Index", ArmarModelo(form));
    }

    var peticion = new Negocio
    {
      IdCategoria = form.IdCategoria,
      IdUsuario = IdUsuarioActual(),
      Nombre = form.Nombre.Trim(),
      Descripcion = string.IsNullOrWhiteSpace(form.Descripcion) ? null : form.Descripcion.Trim(),
      Direccion = form.Direccion.Trim(),
      Ciudad = form.Ciudad.Trim(),
      Telefono = string.IsNullOrWhiteSpace(form.Telefono) ? null : form.Telefono.Trim(),
      Activo = false,
      Estado = EstadoNegocio.Pendiente
    };
    _repoNegocio.Crear(peticion);

    TempData["Mensaje"] = "Tu solicitud fue enviada. Un administrador la va a revisar.";
    return RedirectToAction(nameof(Index));
  }

  private SolicitudesViewModel ArmarModelo(NegocioFormViewModel form)
  {
    return new SolicitudesViewModel
    {
      Form = form,
      Categorias = _repoCategoria.Listar(),
      Solicitudes = _repoNegocio.ListarSolicitudesDeUsuario(IdUsuarioActual())
    };
  }

  private int IdUsuarioActual()
  {
    return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
  }
}