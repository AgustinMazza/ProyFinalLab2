using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservAr.Helpers;
using ReservAr.Models;
using ReservAr.Models.Repositorios;
using ReservAr.ViewModels;

namespace ReservAr.Controllers;

[Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
public class CanchasController : Controller
{
  private const decimal PrecioMaximo = 9999999;

  private static readonly string[] NombresDias =
  {
    "domingo", "lunes", "martes", "miércoles", "jueves", "viernes", "sábado"
  };

  private readonly RepositorioNegocio _repoNegocio;
  private readonly RepositorioRecurso _repoRecurso;
  private readonly RepositorioHorarioAtencion _repoHorario;
  private readonly RepositorioTarifa _repoTarifa;

  public CanchasController(
      RepositorioNegocio repoNegocio,
      RepositorioRecurso repoRecurso,
      RepositorioHorarioAtencion repoHorario,
      RepositorioTarifa repoTarifa)
  {
    _repoNegocio = repoNegocio;
    _repoRecurso = repoRecurso;
    _repoHorario = repoHorario;
    _repoTarifa = repoTarifa;
  }

  [HttpGet]
  public IActionResult Index(int id)
  {
    var negocio = _repoNegocio.ObtenerPorId(id);
    if (negocio == null || negocio.Estado != EstadoNegocio.Aprobado)
      return NotFound();

    if (!PuedeAdministrar(negocio))
      return Forbid();

    ViewBag.IdNegocio = negocio.IdNegocio;
    ViewBag.NegocioNombre = negocio.Nombre;
    return View();
  }

  [HttpGet]
  public IActionResult Listar(int idNegocio)
  {
    var negocio = _repoNegocio.ObtenerPorId(idNegocio);
    if (negocio == null || negocio.Estado != EstadoNegocio.Aprobado)
      return NotFound();

    if (!PuedeAdministrar(negocio))
      return Prohibido();

    var items = _repoRecurso.ListarPorNegocio(idNegocio).Select(r => new
    {
      r.IdRecurso,
      r.Nombre,
      r.Descripcion,
      r.DuracionMin,
      r.Activo,
      Horarios = _repoHorario.ListarPorRecurso(r.IdRecurso).Select(h => new
      {
        h.IdHorarioAtencion,
        h.DiaSemana,
        HoraDesde = FormatearHora(h.HoraDesde),
        HoraHasta = FormatearHora(h.HoraHasta)
      }).ToList(),
      Tarifas = _repoTarifa.ListarPorRecurso(r.IdRecurso).Select(t => new
      {
        t.IdTarifa,
        t.Nombre,
        HoraDesde = FormatearHora(t.HoraDesde),
        HoraHasta = FormatearHora(t.HoraHasta),
        t.PrecioHora,
        t.Activo
      }).ToList()
    }).ToList();

    return Json(items);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult GuardarCancha([FromBody] CanchaFormViewModel model)
  {
    if (!ModelState.IsValid)
      return ErroresDelModelo();

    var negocio = _repoNegocio.ObtenerPorId(model.IdNegocio);
    if (negocio == null || negocio.Estado != EstadoNegocio.Aprobado)
      return NotFound();

    if (!PuedeAdministrar(negocio))
      return Prohibido();

    var nombre = model.Nombre.Trim();
    var descripcion = string.IsNullOrWhiteSpace(model.Descripcion) ? null : model.Descripcion.Trim();

    if (model.IdRecurso == 0)
    {
      if (!PrecioValido(model.PrecioManana) || !PrecioValido(model.PrecioTarde) || !PrecioValido(model.PrecioNoche))
        return Rechazar("Ingresa un precio mayor a 0 para las tres tarifas base");

      var nueva = new Recurso
      {
        IdNegocio = negocio.IdNegocio,
        Nombre = nombre,
        Descripcion = descripcion,
        DuracionMin = model.DuracionMin,
        Activo = true
      };

      var tarifasBase = new List<Tarifa>
      {
        TarifaBase("Mañana", 8, 14, model.PrecioManana),
        TarifaBase("Tarde", 14, 18, model.PrecioTarde),
        TarifaBase("Noche", 18, 24, model.PrecioNoche)
      };

      _repoRecurso.CrearConTarifas(nueva, tarifasBase);
      return Ok(new { ok = true });
    }

    var existente = _repoRecurso.ObtenerPorId(model.IdRecurso);
    if (existente == null || existente.IdNegocio != negocio.IdNegocio)
      return NotFound();

    existente.Nombre = nombre;
    existente.Descripcion = descripcion;
    existente.DuracionMin = model.DuracionMin;
    _repoRecurso.Modificar(existente);

    return Ok(new { ok = true });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult BajaCancha(int id)
  {
    return CambiarActivoCancha(id, false);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult ReactivarCancha(int id)
  {
    return CambiarActivoCancha(id, true);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult GuardarHorario([FromBody] HorarioFormViewModel model)
  {
    if (!ModelState.IsValid)
      return ErroresDelModelo();

    var recurso = RecursoPermitido(model.IdRecurso, out var error);
    if (recurso == null)
      return error!;

    if (!TryParsearHora(model.HoraDesde, out var desde) || !TryParsearHora(model.HoraHasta, out var hasta))
      return Rechazar("Las horas ingresadas no son validas");

    if (desde >= hasta)
      return Rechazar("La hora desde tiene que ser menor a la hora hasta");

    var dias = (model.Dias ?? new List<int>()).Distinct().ToList();
    if (dias.Count == 0 || dias.Any(d => d < 0 || d > 6))
      return Rechazar("Elegi al menos un dia de la semana");

    var esAlta = model.IdHorarioAtencion == 0;
    var existentes = _repoHorario.ListarPorRecurso(recurso.IdRecurso);

    if (!esAlta)
    {
      var actual = _repoHorario.ObtenerPorId(model.IdHorarioAtencion);
      if (actual == null || actual.IdRecurso != recurso.IdRecurso)
        return NotFound();

      if (dias.Count != 1)
        return Rechazar("Al editar un horario elegi un solo dia");

      existentes = existentes.Where(h => h.IdHorarioAtencion != actual.IdHorarioAtencion).ToList();
    }

    foreach (var dia in dias)
    {
      if (existentes.Any(h => h.DiaSemana == dia && h.HoraDesde < hasta && h.HoraHasta > desde))
        return Rechazar($"El horario se superpone con otro del {NombresDias[dia]}");
    }

    if (esAlta)
    {
      foreach (var dia in dias)
      {
        _repoHorario.Crear(new HorarioAtencion
        {
          IdRecurso = recurso.IdRecurso,
          DiaSemana = dia,
          HoraDesde = desde,
          HoraHasta = hasta
        });
      }
    }
    else
    {
      _repoHorario.Modificar(new HorarioAtencion
      {
        IdHorarioAtencion = model.IdHorarioAtencion,
        IdRecurso = recurso.IdRecurso,
        DiaSemana = dias[0],
        HoraDesde = desde,
        HoraHasta = hasta
      });
    }

    return Ok(new { ok = true });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult EliminarHorario(int id)
  {
    var horario = _repoHorario.ObtenerPorId(id);
    if (horario == null)
      return NotFound();

    var recurso = RecursoPermitido(horario.IdRecurso, out var error);
    if (recurso == null)
      return error!;

    _repoHorario.Eliminar(id);
    return Ok(new { ok = true });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult GuardarTarifa([FromBody] TarifaFormViewModel model)
  {
    if (!ModelState.IsValid)
      return ErroresDelModelo();

    var recurso = RecursoPermitido(model.IdRecurso, out var error);
    if (recurso == null)
      return error!;

    if (!TryParsearHora(model.HoraDesde, out var desde) || !TryParsearHora(model.HoraHasta, out var hasta))
      return Rechazar("Las horas ingresadas no son validas");

    if (desde >= hasta)
      return Rechazar("La hora desde tiene que ser menor a la hora hasta");

    if (!PrecioValido(model.PrecioHora))
      return Rechazar("Ingresa un precio mayor a 0");

    var esAlta = model.IdTarifa == 0;
    Tarifa? actual = null;

    if (!esAlta)
    {
      actual = _repoTarifa.ObtenerPorId(model.IdTarifa);
      if (actual == null || actual.IdRecurso != recurso.IdRecurso)
        return NotFound();
    }

    if ((esAlta || actual!.Activo) && SeSuperponeConActivas(recurso.IdRecurso, model.IdTarifa, desde, hasta))
      return Rechazar("La franja se superpone con otra tarifa activa");

    if (esAlta)
    {
      _repoTarifa.Crear(new Tarifa
      {
        IdRecurso = recurso.IdRecurso,
        Nombre = model.Nombre.Trim(),
        HoraDesde = desde,
        HoraHasta = hasta,
        PrecioHora = model.PrecioHora,
        Activo = true
      });
      return Ok(new { ok = true });
    }

    actual!.Nombre = model.Nombre.Trim();
    actual.HoraDesde = desde;
    actual.HoraHasta = hasta;
    actual.PrecioHora = model.PrecioHora;
    _repoTarifa.Modificar(actual);

    return Ok(new { ok = true });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult BajaTarifa(int id)
  {
    return CambiarActivoTarifa(id, false);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult ReactivarTarifa(int id)
  {
    return CambiarActivoTarifa(id, true);
  }

  private IActionResult CambiarActivoCancha(int id, bool activo)
  {
    var recurso = RecursoPermitido(id, out var error);
    if (recurso == null)
      return error!;

    if (activo)
      _repoRecurso.Reactivar(id);
    else
      _repoRecurso.Eliminar(id);

    return Ok(new { ok = true });
  }

  private IActionResult CambiarActivoTarifa(int id, bool activo)
  {
    var tarifa = _repoTarifa.ObtenerPorId(id);
    if (tarifa == null)
      return NotFound();

    var recurso = RecursoPermitido(tarifa.IdRecurso, out var error);
    if (recurso == null)
      return error!;

    if (!activo)
    {
      _repoTarifa.Eliminar(id);
      return Ok(new { ok = true });
    }

    if (SeSuperponeConActivas(tarifa.IdRecurso, tarifa.IdTarifa, tarifa.HoraDesde, tarifa.HoraHasta))
      return Rechazar("No se puede reactivar: la franja se superpone con otra tarifa activa");

    _repoTarifa.Reactivar(id);
    return Ok(new { ok = true });
  }

  private bool SeSuperponeConActivas(int idRecurso, int idTarifaActual, TimeSpan desde, TimeSpan hasta)
  {
    return _repoTarifa.ListarActivasPorRecurso(idRecurso)
        .Any(t => t.IdTarifa != idTarifaActual && t.HoraDesde < hasta && t.HoraHasta > desde);
  }

  private Recurso? RecursoPermitido(int idRecurso, out IActionResult? error)
  {
    error = null;

    var recurso = _repoRecurso.ObtenerPorId(idRecurso);
    var negocio = recurso == null ? null : _repoNegocio.ObtenerPorId(recurso.IdNegocio);

    if (recurso == null || negocio == null)
    {
      error = NotFound();
      return null;
    }

    if (!PuedeAdministrar(negocio))
    {
      error = Prohibido();
      return null;
    }

    return recurso;
  }

  private static Tarifa TarifaBase(string nombre, int horaDesde, int horaHasta, decimal precio)
  {
    return new Tarifa
    {
      Nombre = nombre,
      HoraDesde = TimeSpan.FromHours(horaDesde),
      HoraHasta = TimeSpan.FromHours(horaHasta),
      PrecioHora = precio,
      Activo = true
    };
  }

  private static bool PrecioValido(decimal precio)
  {
    return precio > 0 && precio <= PrecioMaximo;
  }

  private static bool TryParsearHora(string? texto, out TimeSpan hora)
  {
    hora = default;
    texto = (texto ?? "").Trim();

    if (texto == "24:00")
    {
      hora = TimeSpan.FromHours(24);
      return true;
    }

    return TimeSpan.TryParseExact(texto, @"hh\:mm", System.Globalization.CultureInfo.InvariantCulture, out hora);
  }

  private static string FormatearHora(TimeSpan hora)
  {
    return hora.TotalHours >= 24 ? "24:00" : hora.ToString(@"hh\:mm");
  }

  private bool PuedeAdministrar(Negocio negocio)
  {
    return EsSuperAdmin() || negocio.IdUsuario == IdUsuarioActual();
  }

  private bool EsSuperAdmin()
  {
    return User.IsInRole(Roles.SuperAdmin);
  }

  private int IdUsuarioActual()
  {
    return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
  }

  private IActionResult ErroresDelModelo()
  {
    var errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
    return BadRequest(new { errores });
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