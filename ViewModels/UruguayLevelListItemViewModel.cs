namespace UDL.ViewModels;

public sealed class UruguayLevelListItemViewModel
{
    public int PosicionUy { get; set; }
    public int IdNivel { get; init; }
    public string Nombre { get; init; } = "";
    public int? PosicionAredl { get; init; }
    public decimal PuntosAredl { get; init; }
    public int VictorCount { get; init; }
    public int GeometryDashId { get; init; }
    public string Categoria => UDL.Services.Levels.LevelPresentation.Category(PosicionAredl);
    public string? ThumbnailUrl => UDL.Services.Levels.LevelPresentation.Thumbnail(GeometryDashId);
}
