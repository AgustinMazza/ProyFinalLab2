using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservAr.Models.Repositorios;

namespace ReservAr.Controllers;

[AllowAnonymous]
public class CatalogoController : Controller
{
    private const int TamanoPagina = 9;

    private readonly RepositorioNegocio _repoNegocio;
    private readonly RepositorioCategoria _repoCategoria;

    public CatalogoController(RepositorioNegocio repoNegocio, RepositorioCategoria repoCategoria)
    {
        _repoNegocio = repoNegocio;
        _repoCategoria = repoCategoria;
    }

    [HttpGet]
    public IActionResult Listar(string buscar = "", int idCategoria = 0, int pagina = 1)
    {
        buscar = (buscar ?? "").Trim();
        if (pagina < 1) pagina = 1;

        var total = _repoNegocio.ContarPublico(buscar, idCategoria);
        var totalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina);
        var negocios = _repoNegocio.ListarPaginadoPublico(buscar, idCategoria, pagina, TamanoPagina);

        var items = negocios.Select(n => new
        {
            n.IdNegocio,
            n.Nombre,
            n.Descripcion,
            n.Direccion,
            n.Ciudad,
            n.Telefono,
            n.LogoUrl,
            Categoria = n.CategoriaNombre,
            n.CantidadRecursos
        });

        return Json(new { items, total, pagina, totalPaginas });
    }

    [HttpGet]
    public IActionResult ListarCategorias()
    {
        return Json(_repoCategoria.Listar());
    }
}