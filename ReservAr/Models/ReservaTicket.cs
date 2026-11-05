namespace ReservAr.Models;

public class ReservaTicket
{
  public int IdReserva { get; set; }
  public string Cliente { get; set; } = "";
  public string Complejo { get; set; } = "";
  public string Direccion { get; set; } = "";
  public string Ciudad { get; set; } = "";
  public string? Telefono { get; set; }
  public string Cancha { get; set; } = "";
  public DateTime Fecha { get; set; }
  public TimeSpan HoraInicio { get; set; }
  public TimeSpan HoraFin { get; set; }
  public decimal Precio { get; set; }
  public EstadoReserva Estado { get; set; }
}