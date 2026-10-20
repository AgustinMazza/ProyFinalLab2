namespace ReservAr.Models;

public class Tarifa
{
    public int IdTarifa { get; set; }
    public int IdRecurso { get; set; }
    public string Nombre { get; set; } = "";
    public TimeSpan HoraDesde { get; set; }
    public TimeSpan HoraHasta { get; set; }
    public decimal PrecioHora { get; set; }
    public bool Activo { get; set; }
}