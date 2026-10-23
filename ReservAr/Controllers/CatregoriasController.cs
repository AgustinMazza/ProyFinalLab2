using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using ReservAr.Models;
using ReservAr.Models.Repositorios;

namespace ReservAr.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class CategoriasController : Controller
{
  private readonly RepositorioCategoria _repo;

  public CategoriasController(RepositorioCategoria repo)
  {
    _repo = repo;
  }

  //Listar - Read 
  public IActionResult Index()
  {
    var lista = _repo.Listar();
    return View(lista);
  }

  //Crear - Create
  [HttpGet]
  public IActionResult Crear()
  {
    return View();
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Crear(Categoria categoria)
  {
    if (!ModelState.IsValid)
    {
      return View(categoria);
    }

    try
    {
      _repo.Crear(categoria);
    }
    catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
    {
      ModelState.AddModelError(nameof(categoria.Nombre), "Ya existe una categoría con ese nombre");
      return View(categoria);
    }

    return RedirectToAction(nameof(Index));
  }

  //Editar - Update 
  [HttpGet]
  public IActionResult Editar(int id)
  {
    var categoria = _repo.ObtenerPorId(id);
    if (categoria == null)
    {
      return NotFound();
    }

    return View(categoria);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Editar(Categoria categoria)
  {
    if (!ModelState.IsValid)
    {
      return View(categoria);
    }

    try
    {
      _repo.Modificar(categoria);
    }
    catch (MySqlException ex) when (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
    {
      ModelState.AddModelError(nameof(categoria.Nombre), "Ya existe una categoría con ese nombre");
      return View(categoria);
    }

    return RedirectToAction(nameof(Index));
  }

  //Eliminar - Delete 
  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Eliminar(int id)
  {
    try
    {
      _repo.Eliminar(id);
    }
    catch (MySqlException)
    {
      TempData["Error"] = "No se pudo eliminar la categoría. Verificá que no tenga negocios asociados";
    }

    return RedirectToAction(nameof(Index));
  }

}