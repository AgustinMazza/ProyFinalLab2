using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservAr.Helpers;
using ReservAr.Models;
using ReservAr.Models.Repositorios;
using ReservAr.ViewModels;

namespace ReservAr.Controllers;

[Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
public class NegociosController : Controller
{
  private const int TamanoPagina = 10;
  private const int MaxImagenes = 10;
  private const long MaxBytesImagen = 3 * 1024 * 1024;


  private readonly IWebHostEnvironment _env;
  private readonly RepositorioNegocio _repoNegocio;
  private readonly RepositorioCategoria _repoCategoria;
  private readonly RepositorioUsuario _repoUsuario;

  private readonly RepositorioRol _repoRol;

  public NegociosController(
      RepositorioNegocio repoNegocio,
      RepositorioCategoria repoCategoria,
      RepositorioUsuario repoUsuario,
      RepositorioRol repoRol,
      IWebHostEnvironment env)
  {
    _repoNegocio = repoNegocio;
    _repoCategoria = repoCategoria;
    _repoUsuario = repoUsuario;
    _repoRol = repoRol;
    _env = env;
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

    var idDuenio = FiltroDuenio();
    var total = _repoNegocio.Contar(buscar, idDuenio, EstadoNegocio.Aprobado);
    var totalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina);
    var negocios = _repoNegocio.ListarPaginado(buscar, pagina, TamanoPagina, idDuenio, EstadoNegocio.Aprobado);

    var items = negocios.Select(n => new
    {
      n.IdNegocio,
      n.IdCategoria,
      Categoria = n.CategoriaNombre,
      n.IdUsuario,
      Duenio = n.DuenioNombre,
      n.Nombre,
      n.Descripcion,
      n.Direccion,
      n.Ciudad,
      n.Telefono,
      n.Imagenes,
      n.Activo
    });
    return Json(new { items, total, pagina, totalPaginas });

  }

  [HttpGet]
  public IActionResult ListarCategorias()
  {
    return Json(_repoCategoria.Listar());

  }

  [HttpGet]
  [Authorize(Roles = Roles.SuperAdmin)]
  public IActionResult BuscarDuenios(string buscar = "")
  {
    var rolAdmin = _repoRol.ObtenerPorNombre(Roles.Admin);
    if (rolAdmin == null)
      return Json(Array.Empty<object>());

    var duenios = _repoUsuario.BuscarPorRol((buscar ?? "").Trim(), rolAdmin.IdRol, 10)
        .Select(u => new { u.IdUsuario, NombreCompleto = $"{u.Apellido}, {u.Nombre}", u.Email });

    return Json(duenios);
  }

  [HttpGet]
  public IActionResult ListarImagenes(int id)
  {
    var negocio = _repoNegocio.ObtenerPorId(id);
    if (negocio == null)
      return NotFound();

    if (!PuedeAdministrar(negocio))
      return Prohibido();

    return Json(_repoNegocio.ListarImagenes(id));
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> SubirImagenes(int id, List<IFormFile> archivos)
  {
    var negocio = _repoNegocio.ObtenerPorId(id);
    if (negocio == null)
      return NotFound();

    if (!PuedeAdministrar(negocio))
      return Prohibido();

    archivos = archivos?.Where(a => a.Length > 0).ToList() ?? new List<IFormFile>();
    if (archivos.Count == 0)
      return Rechazar("Se necesita minimo una imagen");

    var actuales = _repoNegocio.ContarImagenes(id);
    if (actuales + archivos.Count > MaxImagenes)
      return Rechazar($"Cada negocio puede tener hasta {MaxImagenes} imagenes (ya tiene {actuales})");

    var permitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };

    foreach (var archivo in archivos)
    {
      if (archivo.Length > MaxBytesImagen)
        return Rechazar($"\"{archivo.FileName}\" pesa mas de 3 MB");

      var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
      if (!permitidas.Contains(ext))
        return Rechazar($"\"{archivo.FileName}\" no es JPG, JPEG, PNG o WEBP");

    }
    var carpeta = Path.Combine(_env.WebRootPath, "uploads", "negocios");
    Directory.CreateDirectory(carpeta);


    foreach (var archivo in archivos)
    {
      var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
      var nombreArchivo = $"{Guid.NewGuid()}{extension}";

      using (var stream = new FileStream(Path.Combine(carpeta, nombreArchivo), FileMode.Create))
      {
        await archivo.CopyToAsync(stream);

      }

      _repoNegocio.AgregarImagen(id, $"/uploads/negocios/{nombreArchivo}");

    }
    return Ok(new { ok = true, imagenes = _repoNegocio.ListarImagenes(id) });

  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult EliminarImagen(int id, int idImagen)
  {
    var negocio = _repoNegocio.ObtenerPorId(id);
    if (negocio == null)
      return NotFound();

    if (!PuedeAdministrar(negocio))
      return Prohibido();

    var imagen = _repoNegocio.ObtenerImagen(idImagen);
    if (imagen == null || imagen.IdNegocio != id)
      return NotFound();



    _repoNegocio.EliminarImagen(idImagen);
    BorrarArchivoImagen(imagen.Ruta);

    return Ok(new { ok = true, imagenes = _repoNegocio.ListarImagenes(id) });


  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult HacerpORTADA(int id, int idImagen)
  {
    var negocio = _repoNegocio.ObtenerPorId(id);
    if (negocio == null)
      return NotFound();

    if (!PuedeAdministrar(negocio))
      return Prohibido();

    var imagen = _repoNegocio.ObtenerImagen(idImagen);
    if (imagen == null || imagen.IdNegocio != id)
      return NotFound();

    _repoNegocio.MarcarPortada(id, idImagen);
    return Ok(new { ok = true, imagenes = _repoNegocio.ListarImagenes(id) });



  }




  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Guardar([FromBody] NegocioFormViewModel model)
  {
    var esAlta = model.IdNegocio == 0;

    if (!ModelState.IsValid)
    {
      var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
      return BadRequest(new { errores });
    }


    if (_repoCategoria.ObtenerPorId(model.IdCategoria) == null)
      return Rechazar("La categoria elegida no existe");


    Negocio? existente = null;
    if (!esAlta)
    {
      existente = _repoNegocio.ObtenerPorId(model.IdNegocio);
      if (existente == null)
        return NotFound();

      if (!PuedeAdministrar(existente))
        return Prohibido();
    }

    var idDuenio = ResolverDuenio(model, existente, out var errorDuenio);
    if (errorDuenio != null)
      return Rechazar(errorDuenio);


    var descripcion = string.IsNullOrWhiteSpace(model.Descripcion) ? null : model.Descripcion.Trim();
    var telefono = string.IsNullOrWhiteSpace(model.Telefono) ? null : model.Telefono.Trim();

    if (esAlta)
    {
      var nuevo = new Negocio
      {
        IdCategoria = model.IdCategoria,
        IdUsuario = idDuenio,
        Nombre = model.Nombre.Trim(),
        Descripcion = descripcion,
        Direccion = model.Direccion.Trim(),
        Ciudad = model.Ciudad.Trim(),
        Telefono = telefono,
        Activo = true
      };
      _repoNegocio.Crear(nuevo);
      return Ok(new { ok = true });
    }

    existente!.IdCategoria = model.IdCategoria;
    existente.IdUsuario = idDuenio;
    existente.Nombre = model.Nombre.Trim();
    existente.Descripcion = descripcion;
    existente.Direccion = model.Direccion.Trim();
    existente.Ciudad = model.Ciudad.Trim();
    existente.Telefono = telefono;
    _repoNegocio.Modificar(existente);

    return Ok(new { ok = true });



  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Baja(int id)
  {
    return CambiarActivo(id, false);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Reactivar(int id)
  {
    return CambiarActivo(id, true);
  }




  //metodos privaos



  private IActionResult CambiarActivo(int id, bool activo)
  {
    var negocio = _repoNegocio.ObtenerPorId(id);
    if (negocio == null)
      return NotFound();

    if (!PuedeAdministrar(negocio))
      return Prohibido();

    if (_repoNegocio.CambiarActivo(id, activo) == 0)
      return Rechazar("El negocio no está aprobado");
    return Ok(new { ok = true });
  }

  private int ResolverDuenio(NegocioFormViewModel model, Negocio? existente, out string? error)
  {
    error = null;

    if (!EsSuperAdmin())
      return IdUsuarioActual();

    var idElegido = model.IdUsuario;
    if (idElegido == 0 && existente != null)
      return existente.IdUsuario;

    var duenio = idElegido > 0 ? _repoUsuario.ObtenerPorId(idElegido) : null;
    var rolAdmin = _repoRol.ObtenerPorNombre(Roles.Admin);

    if (duenio == null || rolAdmin == null || duenio.IdRol != rolAdmin.IdRol)
    {
      error = "Elegi un duenio con rol Admin";
      return 0;
    }

    if (!duenio.Activo && (existente == null || existente.IdUsuario != idElegido))
    {
      error = "El duenio elegido esta dado de baja";
      return 0;
    }

    return duenio.IdUsuario;
  }
  private void BorrarArchivoImagen(string? ruta)
  {
    if (string.IsNullOrEmpty(ruta) || !ruta.StartsWith("/uploads/negocios/"))
      return;

    var rutaFisica = Path.Combine(_env.WebRootPath, ruta.TrimStart('/'));
    if (System.IO.File.Exists(rutaFisica))
      System.IO.File.Delete(rutaFisica);
  }

  private bool PuedeAdministrar(Negocio negocio)
  {
    return EsSuperAdmin() || negocio.IdUsuario == IdUsuarioActual();
  }

  private int? FiltroDuenio()
  {
    return EsSuperAdmin() ? null : IdUsuarioActual();
  }

  private bool EsSuperAdmin()
  {
    return User.IsInRole(Roles.SuperAdmin);
  }

  private int IdUsuarioActual()
  {
    return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
  }

  private IActionResult Rechazar(string mensaje)
  {
    return BadRequest(new { errores = new[] { mensaje } });
  }

  private IActionResult Prohibido()
  {
    return StatusCode(StatusCodes.Status403Forbidden, new { errores = new[] { "No tenes permiso para administrar este negocio" } });
  }
}

