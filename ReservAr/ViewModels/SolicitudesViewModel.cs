using ReservAr.Models;

namespace ReservAr.ViewModels;

public class SolicitudesViewModel
{
  public NegocioFormViewModel Form { get; set; } = new();
  public List<Categoria> Categorias { get; set; } = new();
  public List<NegocioListado> Solicitudes { get; set; } = new();
}