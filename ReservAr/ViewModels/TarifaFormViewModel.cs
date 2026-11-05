using System.ComponentModel.DataAnnotations;

namespace ReservAr.ViewModels;

public class TarifaFormViewModel
{
    public int IdTarifa { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Falta la cancha")]
    public int IdRecurso { get; set; }

    [Required(ErrorMessage = "Ingresa el nombre de la tarifa")]
    [StringLength(50)]
    public string Nombre { get; set; } = "";

    public string HoraDesde { get; set; } = "";

    public string HoraHasta { get; set; } = "";

    public decimal PrecioHora { get; set; }
}