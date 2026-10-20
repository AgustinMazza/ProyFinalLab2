using System.ComponentModel.DataAnnotations;

namespace ReservAr.ViewModels;

public class RegistroViewModel
{
    [Required(ErrorMessage = "Ingresá tu nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "Ingresá tu apellido")]
    [StringLength(100)]
    public string Apellido { get; set; } = "";

    [Required(ErrorMessage = "Ingresá tu email")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    [StringLength(150)]
    public string Email { get; set; } = "";

    [StringLength(30)]
    public string? Telefono { get; set; }

    [Required(ErrorMessage = "Ingresá una contraseña")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required(ErrorMessage = "Repetí la contraseña")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    [DataType(DataType.Password)]
    public string ConfirmarPassword { get; set; } = "";
}