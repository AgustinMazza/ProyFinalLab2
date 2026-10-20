using System.ComponentModel.DataAnnotations;

namespace ReservAr.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingresá tu email")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Ingresá tu contraseña")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}