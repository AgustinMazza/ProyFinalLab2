namespace ReservAr.Models;

public class Usuario
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellido { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string? Telefono { get; set; }
    public string? Avatar { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaAlta { get; set; }
}