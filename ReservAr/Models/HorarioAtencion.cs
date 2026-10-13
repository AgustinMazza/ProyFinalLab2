namespace ReservAr.Models;

public class HorarioAtencion
{
    public int IdHorarioAtencion { get; set; }
    public int IdRecurso { get; set; }
    public string? DiaSemana { get; set; }
    public TimeOnly HoraDesde { get; set; }
    public TimeOnly HoraHasta { get; set; }
}