namespace ReservAr.Models;

public class ImagenNegocio
{
    public int IdImagenNegocio { get; set; }
    public int IdNegocio { get; set; }
    public string Ruta { get; set; } = "";
    public string Orden { get; set; } = "";
    public bool EsPortada { get; set; }
}