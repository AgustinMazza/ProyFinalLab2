namespace ReservAr.Models;

public class HorarioAtencion
{
    public int IdHorarioAtencion { get; set; }
    public int IdRecurso { get; set; }
    public int DiaSemana { get; set; }
    public TimeSpan HoraDesde { get; set; }
    public TimeSpan HoraHasta { get; set; }
}