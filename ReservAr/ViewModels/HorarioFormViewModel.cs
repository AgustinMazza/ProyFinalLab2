using System.ComponentModel.DataAnnotations;

namespace ReservAr.ViewModels;

public class HorarioFormViewModel
{
    public int IdHorarioAtencion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Falta la cancha")]
    public int IdRecurso { get; set; }

    public List<int> Dias { get; set; } = new();

    public string HoraDesde { get; set; } = "";

    public string HoraHasta { get; set; } = "";
}