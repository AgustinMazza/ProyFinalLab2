namespace ReservAr.Models;

public class NegocioListado : Negocio
{
    public string CategoriaNombre {get; set; } = "";
    public string DuenioNombre {get; set; } = "" ;
    public string? LogoUrl { get; set; }
    public int CantidadRecursos { get; set; }
}