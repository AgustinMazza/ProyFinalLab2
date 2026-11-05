namespace ReservAr.Models;

public class ReservaGestion : ReservaListado
{
  public string ClienteNombre { get; set; } = "";
  public string? ClienteTelefono { get; set; }
}