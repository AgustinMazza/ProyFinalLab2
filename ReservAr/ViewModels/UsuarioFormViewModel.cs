using System.ComponentModel.DataAnnotations;

namespace ReservAr.ViewModels;

public class UsuarioFormViewModel
{
    public int IdUsuario { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Elegí un rol")]
    public int IdRol { get; set; }

    [Required(ErrorMessage = "Ingresá el nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "Ingresá el apellido")]
    [StringLength(100)]
    public string Apellido { get; set; } = "";

    [Required(ErrorMessage = "Ingresá el email")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    [StringLength(150)]
    public string Email { get; set; } = "";

    [StringLength(30)]
    public string? Telefono { get; set; }

    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
    public string? Password { get; set; }
}