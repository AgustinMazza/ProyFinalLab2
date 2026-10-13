namespace ReservAr.Models;

public class Tarifa
{
    public int IdTarifa { get; set; }
    public int IdRecurso { get; set; }
    public string Nombre { get; set; } = "";
    public TimeOnly HoraDesde { get; set; }
    public TimeOnly HoraHasta { get; set; }
    public decimal PrecioHora { get; set; }
    public bool Activo { get; set; }
}