using ReservAr.Models;

namespace ReservAr.ViewModels;

public class NegocioReservarViewModel
{
    public Negocio Negocio { get; set; } = new();

    public List<Recurso> Recursos { get; set;} = new();
    
}