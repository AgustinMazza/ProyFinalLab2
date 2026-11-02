using System.ComponentModel.DataAnnotations;

namespace ReservAr.ViewModels;

public class LoginApiRequest
{
  [Required]
  public string Email { get; set; } = "";

  [Required]
  public string Password { get; set; } = "";
}