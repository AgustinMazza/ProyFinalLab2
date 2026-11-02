using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservAr.Models.Repositorios;

namespace ReservAr.Controllers.Api;

[ApiController]
[Route("api/negocios")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class NegociosApiController : ControllerBase
{
  private const int TamanoPagina = 10;

  private readonly RepositorioNegocio _repoNegocio;

  public NegociosApiController(RepositorioNegocio repoNegocio)
  {
    _repoNegocio = repoNegocio;
  }

  [HttpGet]
  public IActionResult Listar(string buscar = "", int idCategoria = 0, int pagina = 1)
  {
    buscar = (buscar ?? "").Trim();
    if (pagina < 1) pagina = 1;

    var total = _repoNegocio.ContarPublico(buscar, idCategoria);
    var totalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina);
    var items = _repoNegocio.ListarPaginadoPublico(buscar, idCategoria, pagina, TamanoPagina)
        .Select(n => new
        {
          n.IdNegocio,
          n.Nombre,
          n.Descripcion,
          n.Direccion,
          n.Ciudad,
          n.Telefono,
          Categoria = n.CategoriaNombre,
          n.CantidadRecursos
        });

    return Ok(new { items, total, pagina, totalPaginas });
  }
}