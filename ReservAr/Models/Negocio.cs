using Microsoft.AspNetCore.SignalR;

namespace ReservAr.Models;

public class Negocio
{
    public int IdNegocio { get; set; }
    public int IdCategoria { get; set; }
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Telefono { get; set; }
    public string? LogoUrl { get; set; }
    public bool Activo { get; set; }
}