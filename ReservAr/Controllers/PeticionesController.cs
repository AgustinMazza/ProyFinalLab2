using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservAr.Helpers;
using ReservAr.Models;
using ReservAr.Models.Repositorios;

namespace ReservAr.Controllers;

[Authorize(Roles = Roles.SuperAdmin)]
public class PeticionesController : Controller
{
  private const int TamanoPagina = 10;

  private readonly RepositorioNegocio _repoNegocio;
  private readonly RepositorioRol _repoRol;

  public PeticionesController(RepositorioNegocio repoNegocio, RepositorioRol repoRol)
  {
    _repoNegocio = repoNegocio;
    _repoRol = repoRol;
  }

  public IActionResult Index()
  {
    return View();
  }

  [HttpGet]
  public IActionResult Listar(string buscar = "", int pagina = 1)
  {
    buscar = (buscar ?? "").Trim();
    if (pagina < 1) pagina = 1;

    var total = _repoNegocio.Contar(buscar, null, EstadoNegocio.Pendiente);
    var totalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina);
    var peticiones = _repoNegocio.ListarPaginado(buscar, pagina, TamanoPagina, null, EstadoNegocio.Pendiente);

    var items = peticiones.Select(n => new
    {
      n.IdNegocio,
      n.Nombre,
      Categoria = n.CategoriaNombre,
      n.Direccion,
      n.Ciudad,
      n.Telefono,
      n.Descripcion,
      Solicitante = n.DuenioNombre
    });

    return Json(new { items, pagina, totalPaginas });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Aprobar(int id)
  {
    var rolAdmin = _repoRol.ObtenerPorNombre(Roles.Admin);
    if (rolAdmin == null)
      return BadRequest(new { errores = new[] { "No existe el rol Admin" } });

    if (!_repoNegocio.Aprobar(id, rolAdmin.IdRol))
      return BadRequest(new { errores = new[] { "La petición ya no está pendiente" } });

    return Ok(new { ok = true });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Rechazar(int id)
  {
    if (!_repoNegocio.Rechazar(id))
      return BadRequest(new { errores = new[] { "La petición ya no está pendiente" } });

    return Ok(new { ok = true });
  }
}