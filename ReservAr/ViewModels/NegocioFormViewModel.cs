using System.ComponentModel.DataAnnotations;

namespace ReservAr.ViewModels;

public class NegocioFormViewModel
{
    public int IdNegocio { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Elegi una categoria ")]
    public int IdCategoria { get; set; }

    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "Ingresa el nombre ")]
    [StringLength(150)]
    public string Nombre { get; set; } = "";


    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "Ingresa la direccion")]
    [StringLength(200)]
    public string Direccion { get; set; } = "";

    [Required(ErrorMessage = "Ingresa la ciudad")]
    [StringLength(100)]
    public string Ciudad { get; set; } = "";

    [StringLength(30)]
    public string? Telefono { get; set; }


}