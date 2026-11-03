using ReservAr.Models;

namespace ReservAr.ViewModels;


public class DetalleReservarViewModel
{
    public Negocio Negocio {get; set; } = new();

    public Recurso Recurso {get; set;} = new();

    public List<HorarioAtencion> Horarios {get; set; } = new();

    public List<Tarifa> Tarifas {get; set; } = new();


}