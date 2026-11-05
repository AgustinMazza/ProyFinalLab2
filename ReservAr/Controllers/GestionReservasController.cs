using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservAr.Helpers;
using ReservAr.Models;
using ReservAr.Models.Repositorios;

namespace ReservAr.Controllers;

[Authorize(Roles = Roles.Admin + "," + Roles.SuperAdmin)]
public class GestionReservasController : Controller
{
  private const int TamanoPagina = 10;

  private readonly RepositorioReserva _repoReserva;
  private readonly RepositorioNegocio _repoNegocio;

  public GestionReservasController(RepositorioReserva repoReserva, RepositorioNegocio repoNegocio)
  {
    _repoReserva = repoReserva;
    _repoNegocio = repoNegocio;
  }

  public IActionResult Index()
  {
    return View();
  }

  [HttpGet]
  public IActionResult Listar(string buscar = "", int estado = -1, int pagina = 1)
  {
    buscar = (buscar ?? "").Trim();
    if (pagina < 1) pagina = 1;

    EstadoReserva? filtroEstado = (estado >= 0 && estado <= 3) ? (EstadoReserva)estado : null;
    int? idDuenio = EsSuperAdmin() ? null : IdUsuarioActual();

    var total = _repoReserva.ContarDeNegocios(buscar, idDuenio, filtroEstado);
    var totalPaginas = (int)Math.Ceiling(total / (double)TamanoPagina);
    var reservas = _repoReserva.ListarDeNegocios(buscar, idDuenio, filtroEstado, pagina, TamanoPagina);

    var ahora = DateTime.Now;
    var items = reservas.Select(r => new
    {
      r.IdReserva,
      Cliente = r.ClienteNombre,
      Telefono = r.ClienteTelefono,
      Lugar = $"{r.NegocioNombre} - {r.RecursoNombre}",
      Fecha = r.Fecha.ToString("yyyy-MM-dd"),
      HoraInicio = ReservasController.FormatearHora(r.HoraInicio),
      HoraFin = ReservasController.FormatearHora(r.HoraFin),
      r.Precio,
      Estado = ReservasController.CalcularEstado(r, ahora),
      PuedeConfirmar = PuedeConfirmar(r, ahora),
      PuedeCancelar = ReservasController.PuedeCancelar(r, ahora)
    });

    return Json(new { items, pagina, totalPaginas });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Confirmar(int id)
  {
    var reserva = ObtenerAdministrable(id);
    if (reserva == null)
      return NotFound(new { mensaje = "La reserva no existe" });

    if (!PuedeConfirmar(reserva, DateTime.Now))
      return BadRequest(new { mensaje = "Esta reserva ya no se puede confirmar" });

    _repoReserva.CambiarEstado(id, EstadoReserva.Confirmada);
    return Ok(new { mensaje = "Reserva confirmada" });
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public IActionResult Cancelar(int id)
  {
    var reserva = ObtenerAdministrable(id);
    if (reserva == null)
      return NotFound(new { mensaje = "La reserva no existe" });

    if (!ReservasController.PuedeCancelar(reserva, DateTime.Now))
      return BadRequest(new { mensaje = "Esta reserva ya no se puede cancelar" });

    _repoReserva.CambiarEstado(id, EstadoReserva.Cancelada);
    return Ok(new { mensaje = "Reserva cancelada" });
  }

  private Reserva? ObtenerAdministrable(int id)
  {
    var reserva = _repoReserva.ObtenerPorId(id);
    if (reserva == null) return null;
    if (EsSuperAdmin()) return reserva;

    var negocio = _repoNegocio.ObtenerPorId(reserva.IdNegocio);
    return negocio != null && negocio.IdUsuario == IdUsuarioActual() ? reserva : null;
  }

  private static bool PuedeConfirmar(Reserva r, DateTime ahora)
  {
    return r.Estado == EstadoReserva.Pendiente && r.Fecha.Date + r.HoraInicio > ahora;
  }

  private bool EsSuperAdmin() => User.IsInRole(Roles.SuperAdmin);

  private int IdUsuarioActual() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}