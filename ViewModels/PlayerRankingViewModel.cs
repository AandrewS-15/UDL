namespace UDL.ViewModels;

public sealed class PlayerRankingViewModel
{
    public int Posicion { get; set; }
    public int IdJugador { get; init; }
    public string NombreGD { get; init; } = "";
    public string? AvatarUrl { get; init; }
    public decimal TotalPuntos { get; init; }
    public int CantidadCompletions { get; init; }
}
