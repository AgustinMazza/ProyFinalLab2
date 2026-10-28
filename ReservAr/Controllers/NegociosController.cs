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


    private readonly RepositorioNegocio _repoNegocio;
    private readonly RepositorioCategoria _repoCategoria;
    private readonly RepositorioUsuario _repoUsuario;

    private readonly RepositorioRol _repoRol;

    public NegociosController(
        RepositorioNegocio repoNegocio,
        RepositorioCategoria repoCategoria,
        RepositorioUsuario repoUsuario,
        RepositorioRol repoRol)
    {
        _repoNegocio = repoNegocio;
        _repoCategoria = repoCategoria;
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

        var idDuenio = FiltroDuenio();
        var total = _repoNegocio.Contar(buscar, idDuenio);
        var totalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina);
        var negocios = _repoNegocio.ListarPaginado(buscar, pagina, TamanoPagina, idDuenio);

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
    public IActionResult ListarDuenios()
    {
        var rolAdmin = _repoRol.ObtenerPorNombre(Roles.Admin);
        if (rolAdmin == null)
            return Json(Array.Empty<object>());
        var duenios = _repoUsuario.Listar()
        .Where(u => u.IdRol == rolAdmin.IdRol)
        .Select(u => new { u.IdUsuario, NombreCompleto = $"{u.Apellido}, {u.Nombre}" });

        return Json(duenios);



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
        return CambiarEstado(id, false);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reactivar(int id)
    {
        return CambiarEstado(id, true);
    }




    //metodos privaos



    private IActionResult CambiarEstado(int id, bool activo)
    {
        var negocio = _repoNegocio.ObtenerPorId(id);
        if (negocio == null)
            return NotFound();

        if (!PuedeAdministrar(negocio))
            return Prohibido();

        _repoNegocio.CambiarEstado(id, activo);
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

