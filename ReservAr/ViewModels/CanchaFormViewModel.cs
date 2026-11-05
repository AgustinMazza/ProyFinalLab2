using System.ComponentModel.DataAnnotations;

namespace ReservAr.ViewModels;

public class CanchaFormViewModel
{
    public int IdRecurso { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Falta el negocio")]
    public int IdNegocio { get; set; }

    [Required(ErrorMessage = "Ingresa el nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = "";

    [StringLength(300)]
    public string? Descripcion { get; set; }

    [Range(60, 120, ErrorMessage = "La duracion del turno debe estar entre 60 y 120 minutos")]
    public int DuracionMin { get; set; } = 60;

    public decimal PrecioManana { get; set; }
    public decimal PrecioTarde { get; set; }
    public decimal PrecioNoche { get; set; }
}