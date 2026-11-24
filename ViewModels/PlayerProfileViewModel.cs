namespace UDL.ViewModels;

public sealed class PlayerProfileViewModel
{
    public int IdJugador { get; init; }
    public string NombreGD { get; init; } = "";
    public string? AvatarUrl { get; init; }
    public int PosicionUy { get; init; }
    public decimal TotalPuntos { get; init; }
    public int CantidadCompletions { get; init; }
    public IReadOnlyList<UruguayLevelListItemViewModel> Completions { get; init; } = [];
    public UruguayLevelListItemViewModel? Hardest => Completions.FirstOrDefault();
}
