namespace ReservAr.Models;

public class ImagenNegocio
{
    public int IdImagenNegocio { get; set; }
    public int IdNegocio { get; set; }
    public string Ruta { get; set; } = "";
    public int Orden { get; set; } 
    public bool esPortada { get; set; }
}