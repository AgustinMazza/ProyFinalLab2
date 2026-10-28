using System.ComponentModel.DataAnnotations;

namespace ReservAr.ViewModels;

public class CambiarPasswordViewModel
{
  [Required(ErrorMessage = "Ingresá tu contraseña actual")]
  [DataType(DataType.Password)]
  public string ActualPassword { get; set; } = "";

  [Required(ErrorMessage = "Ingresá tu nueva contraseña")]
  [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
  [DataType(DataType.Password)]
  public string NuevaPassword { get; set; } = "";

  [Required(ErrorMessage = "Repetí la nueva contraseña")]
  [Compare(nameof(NuevaPassword), ErrorMessage = "Las contraseñas no coinciden, repetí tu nueva contraseña")]
  [DataType(DataType.Password)]
  public string ConfirmarPassword { get; set; } = "";
}