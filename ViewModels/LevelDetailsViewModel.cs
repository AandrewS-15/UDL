using UDL.Services.Levels;

namespace UDL.ViewModels;

public sealed class LevelDetailsViewModel
{
    public int IdNivel { get; init; }
    public string Nombre { get; init; } = "";
    public int? PosicionAredl { get; init; }
    public int? PosicionUy { get; set; }
    public string Categoria => LevelPresentation.Category(PosicionAredl);
    public decimal PuntosAredl { get; init; }
    public string? Publisher { get; init; }
    public int GeometryDashId { get; init; }
    public bool TwoPlayer { get; init; }
    public string? ThumbnailUrl => LevelPresentation.Thumbnail(GeometryDashId);
    public int VictorCount => Victors.Count;
    public IReadOnlyList<PlayerRankingViewModel> Victors { get; set; } = [];
    public string? Description { get; init; }
    public string? TagsRaw { get; init; }
    public IReadOnlyList<string> Tags => LevelPresentation.Tags(TagsRaw);
    public int? Song { get; init; }
    public decimal? EDEL { get; init; }
    public decimal? GDDL { get; init; }
    public string? NLW { get; init; }
}
