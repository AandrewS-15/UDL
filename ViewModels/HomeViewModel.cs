namespace UDL.ViewModels;

public sealed class HomeViewModel
{
    public IReadOnlyList<PlayerRankingViewModel> TopPlayers { get; init; } = [];
    public IReadOnlyList<HomeRecordViewModel> RecentRecords { get; init; } = [];
    public IReadOnlyList<HomeLevelChangeViewModel> RecentLevelChanges { get; init; } = [];
}

public sealed class HomeRecordViewModel
{
    public int IdRecord { get; init; }
    public int IdJugador { get; init; }
    public int IdNivel { get; init; }
    public string NombreGD { get; init; } = "";
    public string NombreNivel { get; init; } = "";
    public DateTime FechaAprobacion { get; init; }
}

// Contrato reservado; su contenido se definirá junto con la persistencia de eventos UDL.
public sealed record HomeLevelChangeViewModel;
