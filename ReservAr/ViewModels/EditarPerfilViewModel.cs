using System.ComponentModel.DataAnnotations;

namespace ReservAr.ViewModels;

public class EditarPerfilViewModel
{
  [Required(ErrorMessage = "Ingresá tu nombre")]
  [StringLength(100)]
  public string Nombre { get; set; } = "";

  [Required(ErrorMessage = "Ingresá tu apellido")]
  [StringLength(100)]
  public string Apellido { get; set; } = "";

  [StringLength(30)]
  public string? Telefono { get; set; }
}