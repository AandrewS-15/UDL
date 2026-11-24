using System.ComponentModel.DataAnnotations;

namespace UDL.ViewModels;

public sealed class RegisterViewModel
{
    [Required, StringLength(50, MinimumLength = 3), Display(Name = "Nombre de usuario")]
    public string UserName { get; set; } = "";
    [Required, EmailAddress, StringLength(256)] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password), StringLength(128, MinimumLength = 12), Display(Name = "Contraseña")]
    public string Password { get; set; } = "";
    [Required, DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = "";
}
public sealed class LoginViewModel
{
    [Required, StringLength(256), Display(Name = "Email o nombre de usuario")] public string Login { get; set; } = "";
    [Required, DataType(DataType.Password), StringLength(128), Display(Name = "Contraseña")] public string Password { get; set; } = "";
    [Display(Name = "Recordar sesión")] public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}
public sealed class PlayerChoiceViewModel
{
    public int IdJugador { get; init; }
    public string NombreGD { get; init; } = "";
}
public sealed class AccountViewModel
{
    public string UserName { get; init; } = "";
    public string Email { get; init; } = "";
    public PlayerChoiceViewModel? Player { get; init; }
    public bool Verified { get; init; }
    public string Search { get; init; } = "";
    public IReadOnlyList<PlayerChoiceViewModel> AvailablePlayers { get; init; } = [];
    public string? Message { get; init; }
}
