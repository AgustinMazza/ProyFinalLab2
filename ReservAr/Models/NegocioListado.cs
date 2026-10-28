namespace ReservAr.Models;

public class NegocioListado
{
    public int IdNegocio { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Telefono { get; set; }
    public string? LogoUrl { get; set; }
    public string Categoria { get; set; } = "";
    public int CantidadRecursos { get; set; }
}