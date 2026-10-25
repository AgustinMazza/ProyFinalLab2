namespace ReservAr.Models;

public class Reserva
{
  public int IdReserva { get; set; }
  public int IdUsuario { get; set; }
  public int IdRecurso { get; set; }
  public int IdNegocio { get; set; }
  public DateTime Fecha { get; set; }
  public TimeSpan HoraInicio { get; set; }
  public TimeSpan HoraFin { get; set; }
  public EstadoReserva Estado { get; set; }
  public decimal Precio { get; set; }
  public string? Observaciones { get; set; }
  public string? Comprobante { get; set; }
  public DateTime FechaCreacion { get; set; }
}