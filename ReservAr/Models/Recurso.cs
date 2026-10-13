namespace ReservAr.Models;

public class Recurso
{
    public int IdRecurso { get; set; }
    public int IdNegocio { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }

    public int DuracionMin { get; set; }

    public bool Activo { get; set; }

}