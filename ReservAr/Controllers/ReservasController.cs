using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservAr.Models;
using ReservAr.Models.Repositorios;
using ReservAr.ViewModels;
using System.Security.Claims;
using ReservAr.Helpers;


namespace ReservAr.Controllers;

public class ReservasController : Controller
{
  private readonly RepositorioNegocio _repoNegocio;
  private readonly RepositorioRecurso _repoRecurso;
  private readonly RepositorioReserva _repoReserva;
  private readonly RepositorioHorarioAtencion _repoHorario;
  private readonly RepositorioTarifa _repoTarifa;
  private const int TamanoPagina = 10;

  public ReservasController(
      RepositorioReserva repoReserva,
      RepositorioNegocio repoNegocio,
      RepositorioRecurso repoRecurso,
      RepositorioHorarioAtencion repoHorario,
      RepositorioTarifa repoTarifa)
  {
    _repoReserva = repoReserva;
    _repoNegocio = repoNegocio;
    _repoRecurso = repoRecurso;
    _repoHorario = repoHorario;
    _repoTarifa = repoTarifa;
  }

  [AllowAnonymous]
  [HttpGet]
  public IActionResult Negocio(int id)
  {
    var negocio = _repoNegocio.ObtenerPorId(id);

    if (negocio == null || !negocio.Activo)
    {
      return NotFound();

    }
    var recursos = _repoRecurso
    .ListarPorNegocio(id)
    .Where(r => r.Activo)
    .ToList();

    var modelo = new NegocioReservarViewModel
    {
      Negocio = negocio,
      Recursos = recursos
    };

    return View(modelo);



  }

  [AllowAnonymous]
  [HttpGet]
  public IActionResult Detalles(int id)
  {
    var recurso = _repoRecurso.ObtenerPorId(id);

    if (recurso == null || !recurso.Activo)
    {
      return NotFound();
    }

    var negocio = _repoNegocio.ObtenerPorId(recurso.IdNegocio);

    if (negocio == null || !negocio.Activo)
    {
      return NotFound();
    }

    var modelo = new DetalleReservarViewModel
    {
      Negocio = negocio,
      Recurso = recurso,
      Horarios = _repoHorario.ListarPorRecurso(id),
      Tarifas = _repoTarifa.ListarPorRecurso(id)
        .Where(t => t.Activo)
        .ToList()
    };
    return View(modelo);

  }
  [AllowAnonymous]
  [HttpGet]
  public IActionResult Disponibilidad(int id, DateTime fecha)
  {

    if (fecha.Date < DateTime.Today)
      return BadRequest(new { mensaje = "no podes consultar fechas pasadas" });

    var recurso = _repoRecurso.ObtenerPorId(id);

    if (recurso == null || !recurso.Activo)
    {
      return NotFound();

    }

    var horarios = _repoHorario.ListarPorRecurso(id)
    .Where(h => h.DiaSemana == (int)fecha.DayOfWeek)
    .ToList();


    var tarifas = _repoTarifa.ListarPorRecurso(id)
    .Where(t => t.Activo)
    .ToList();

    var reservas = _repoReserva.ListarPorRecursoYFecha(id, fecha);

    var turnos = new List<object>();

    foreach (var horario in horarios)
    {
      var inicio = horario.HoraDesde;

      while (inicio + TimeSpan.FromMinutes(recurso.DuracionMin) <= horario.HoraHasta)
      {
        var fin = inicio + TimeSpan.FromMinutes(recurso.DuracionMin);

        bool ocupado = reservas.Any(r =>
        r.HoraInicio < fin &&
        r.HoraFin > inicio);

        bool yaPaso = fecha.Date == DateTime.Today && inicio <= DateTime.Now.TimeOfDay;

        if (!ocupado && !yaPaso)
        {
          var precio = CalcularPrecio(inicio, fin, tarifas);
          if (precio.HasValue)
            turnos.Add(new
            {
              horaInicio = inicio.ToString(@"hh\:mm"),
              horaFin = fin.ToString(@"hh\:mm"),
              precio = precio.Value
            });
        }
        inicio = fin;
      }

    }
    return Json(turnos);


  }

  [Authorize]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Confirmar(
      int idRecurso,
      DateTime fecha,
      TimeSpan horaInicio)
  {
    var idUsuarioTexto = User.FindFirstValue(ClaimTypes.NameIdentifier);

    if (!int.TryParse(idUsuarioTexto, out int idUsuario))
      return Unauthorized(new { mensaje = "Debes iniciar sesion" });

    if (fecha.Date < DateTime.Today)
      return BadRequest(new { mensaje = "No podes reservar fechas pasadas" });

    var recurso = _repoRecurso.ObtenerPorId(idRecurso);

    if (recurso == null || !recurso.Activo || recurso.DuracionMin <= 0)
      return NotFound(new { mensaje = "El recurso no esta disponible" });

    var negocio = _repoNegocio.ObtenerPorId(recurso.IdNegocio);

    if (negocio == null || !negocio.Activo)
      return NotFound(new { mensaje = "El establecimiento no esta disponible" });

    var horaFin = horaInicio.Add(
        TimeSpan.FromMinutes(recurso.DuracionMin));

    var horarios = _repoHorario.ListarPorRecurso(idRecurso)
        .Where(h => h.DiaSemana == (int)fecha.DayOfWeek)
        .ToList();

    bool turnoValido = horarios.Any(h =>
    {
      var inicio = h.HoraDesde;

      while (inicio + TimeSpan.FromMinutes(recurso.DuracionMin)
                <= h.HoraHasta)
      {
        if (inicio == horaInicio)
          return true;

        inicio += TimeSpan.FromMinutes(recurso.DuracionMin);
      }

      return false;
    });

    if (!turnoValido)
      return BadRequest(new { mensaje = "El horario seleccionado no es valido" });

    if (fecha.Date == DateTime.Today && horaInicio <= DateTime.Now.TimeOfDay)
      return BadRequest(new { mensaje = "Ese horario ya pasó" });
    var reservas = _repoReserva.ListarPorRecursoYFecha(idRecurso, fecha);

    bool ocupado = reservas.Any(r =>
        r.HoraInicio < horaFin &&
        r.HoraFin > horaInicio);

    if (ocupado)
      return Conflict(new { mensaje = "El turno ya fue reservado" });

    var tarifas = _repoTarifa.ListarPorRecurso(idRecurso)
        .Where(t => t.Activo)
        .ToList();

    var precio = CalcularPrecio(horaInicio, horaFin, tarifas);

    if (!precio.HasValue)
      return BadRequest(new { mensaje = "No se pudo calcular el precio del turno" });

    var reserva = new Reserva
    {
      IdUsuario = idUsuario,
      IdRecurso = idRecurso,
      IdNegocio = recurso.IdNegocio,
      Fecha = fecha.Date,
      HoraInicio = horaInicio,
      HoraFin = horaFin,
      Estado = EstadoReserva.Pendiente,
      Precio = precio.Value,
      FechaCreacion = DateTime.Now
    };

    int idReserva = _repoReserva.Crear(reserva);

    return Ok(new
    {
      mensaje = "Reserva creada correctamente",
      idReserva = idReserva
    });
  }

  [Authorize(Roles = Roles.Usuario)]
  [HttpGet]
  public IActionResult Mias()
  {
    return View();
  }

  [Authorize(Roles = Roles.Usuario)]
  [HttpGet]
  public IActionResult ListarMias(int pagina = 1)
  {
    if (pagina < 1) pagina = 1;

    var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    var ahora = DateTime.Now;

    var total = _repoReserva.ContarDeUsuario(idUsuario);
    var totalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina);

    var items = _repoReserva.ListarDeUsuario(idUsuario, pagina, TamanoPagina)
        .Select(r => new
        {
          r.IdReserva,
          Lugar = $"{r.NegocioNombre} - {r.RecursoNombre}",
          Fecha = r.Fecha.ToString("yyyy-MM-dd"),
          HoraInicio = FormatearHora(r.HoraInicio),
          HoraFin = FormatearHora(r.HoraFin),
          Estado = CalcularEstado(r, ahora),
          PuedeCancelar = PuedeCancelar(r, ahora),
          PuedeTicket = r.Estado == EstadoReserva.Confirmada || r.Estado == EstadoReserva.Completada
        });

    return Json(new { items, total, pagina, totalPaginas });
  }

  [Authorize(Roles = Roles.Usuario)]
  [HttpGet]
  public IActionResult InfoMia(int id)
  {
    var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    var info = _repoReserva.ObtenerInfoDeUsuario(id, idUsuario);
    if (info == null)
      return NotFound();

    return Json(new
    {
      info.Complejo,
      info.Direccion,
      info.Ciudad,
      info.DescripcionComplejo,
      info.Telefono,
      info.Cancha,
      info.DescripcionCancha
    });
  }

  [Authorize(Roles = Roles.Usuario)]
  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Cancelar(int id)
  {
    var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    var reserva = _repoReserva.ObtenerPorId(id);
    if (reserva == null || reserva.IdUsuario != idUsuario)
      return NotFound(new { mensaje = "La reserva no existe" });

    if (!PuedeCancelar(reserva, DateTime.Now))
      return BadRequest(new { mensaje = "Esta reserva ya no se puede cancelar" });

    _repoReserva.CambiarEstado(id, EstadoReserva.Cancelada);
    return Ok(new { mensaje = "Reserva cancelada" });
  }

  [Authorize(Roles = Roles.Usuario)]
  [HttpGet]
  public IActionResult Ticket(int id)
  {
    var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    var ticket = _repoReserva.ObtenerTicketDeUsuario(id, idUsuario);
    if (ticket == null ||
        (ticket.Estado != EstadoReserva.Confirmada && ticket.Estado != EstadoReserva.Completada))
      return NotFound();

    return View(ticket);
  }

  private static string CalcularEstado(Reserva r, DateTime ahora)
  {
    if (r.Estado == EstadoReserva.Cancelada)
      return "Cancelada";

    var yaPaso = r.Fecha.Date + r.HoraFin <= ahora;

    if (r.Estado == EstadoReserva.Pendiente)
      return yaPaso ? "Vencida" : "Pendiente";

    return yaPaso ? "Finalizada" : "Confirmada";
  }

  private static bool PuedeCancelar(Reserva r, DateTime ahora)
  {
    var activa = r.Estado == EstadoReserva.Pendiente || r.Estado == EstadoReserva.Confirmada;
    return activa && r.Fecha.Date + r.HoraInicio > ahora;
  }
  private static string FormatearHora(TimeSpan hora)
  {
    return hora.TotalHours >= 24 ? "24:00" : hora.ToString(@"hh\:mm");
  }
  private decimal? CalcularPrecio(
      TimeSpan inicio,
      TimeSpan fin,
      List<Tarifa> tarifas)
  {
    decimal total = 0;
    var horaActual = inicio;

    while (horaActual < fin)
    {
      var tarifa = tarifas
      .Where(t =>
  t.HoraDesde <= horaActual &&
  t.HoraHasta > horaActual)
  .OrderBy(t => t.HoraHasta)
  .FirstOrDefault();


      if (tarifa == null)
        return null;

      var finTramo = tarifa.HoraHasta < fin
      ? tarifa.HoraHasta
      : fin;

      var minutos = (decimal)(finTramo - horaActual).TotalMinutes;

      total += (minutos / 60m) * tarifa.PrecioHora;
      horaActual = finTramo;


    }
    return Math.Round(total, 2);


  }






}