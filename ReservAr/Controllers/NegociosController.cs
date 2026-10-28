using Microsoft.AspNetCore.Mvc;
using ReservAr.Models.Repositorios;

namespace ReservAr.Controllers;

public class NegociosController : Controller
{
  private const int TamanoPagina = 9;

  private readonly RepositorioNegocio _repoNegocio;
  private readonly RepositorioCategoria _repoCategoria;

  public NegociosController(RepositorioNegocio repoNegocio, RepositorioCategoria repoCategoria)
  {
    _repoNegocio = repoNegocio;
    _repoCategoria = repoCategoria;
  }

  [HttpGet]
  public IActionResult Listar(string buscar = "", int idCategoria = 0, int pagina = 1)
  {
    buscar = (buscar ?? "").Trim();
    if (pagina < 1) pagina = 1;

    var total = _repoNegocio.Contar(buscar, idCategoria);
    var totalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina);
    var items = _repoNegocio.ListarPaginado(buscar, idCategoria, pagina, TamanoPagina);

    return Json(new { items, total, pagina, totalPaginas });
  }

  [HttpGet]
  public IActionResult ListarCategorias()
  {
    return Json(_repoCategoria.Listar());
  }
}