namespace ReservAr.Models;

public class NegocioListado : Negocio
{
  public string CategoriaNombre { get; set; } = "";
  public string DuenioNombre { get; set; } = "";
  public int CantidadRecursos { get; set; }
  public string? CategoriaIcono { get; set; }
  public List<string> Imagenes { get; set; } = new(); 
}