namespace ReservAr.Models;

public class ReservaInfo
{
  public string Complejo { get; set; } = "";
  public string Direccion { get; set; } = "";
  public string Ciudad { get; set; } = "";
  public string? DescripcionComplejo { get; set; }
  public string? Telefono { get; set; }
  public string Cancha { get; set; } = "";
  public string? DescripcionCancha { get; set; }
}