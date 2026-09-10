namespace UDL.Services.Import;

public sealed record ExistingPlayer(int Id, string Username, string? AvatarUrl);
public sealed record ExistingLevel(int Id, Guid AredlId, string Name, int? Position, bool TwoPlayer,
    int? GeometryDashId = null, string? Publisher = null);
public sealed record ExistingRecord(int PlayerId, int LevelId, byte Percentage);
public sealed record ImportDatabase(bool HasAvatarColumn, IReadOnlyList<ExistingPlayer> Players,
    IReadOnlyList<ExistingLevel> Levels, IReadOnlyList<ExistingRecord> Records);
public sealed record PlayerPlan(string Username, string Key, string Status, int? ExistingId,
    string? AvatarUrl, string AvatarAction, IReadOnlyList<int> SourceRows);
public sealed record LevelPlan(int Row, string Name, int? HistoricalRank, string Status,
    int? LevelId, IReadOnlyList<ExistingLevel> Candidates, string? Resolution = null);
public sealed record RecordPlan(int Row, string Username, string LevelName, int? LevelId,
    byte Percentage, string Status, string? Reason, string? CanonicalUsername = null);
public sealed record ImportConflict(string Type, int Row, string Name, int? HistoricalRank,
    string Reason, IReadOnlyList<string> Candidates, bool? AppearsInPlayers = null,
    IReadOnlyList<string>? PlayersCandidates = null, IReadOnlyList<string>? SqlCandidates = null);
public sealed record ImportWarning(string Type, int Row, string Name, string Message);

public sealed class ImportPlan
{
    public required string SourceUrl { get; init; }
    public required DateTimeOffset CapturedAtUtc { get; init; }
    public required bool HasAvatarColumn { get; init; }
    public int DetectedPlayers { get; init; }
    public List<PlayerPlan> Players { get; } = [];
    public List<LevelPlan> Levels { get; } = [];
    public List<RecordPlan> Records { get; } = [];
    public List<ImportConflict> Conflicts { get; } = [];
    public List<ImportWarning> Warnings { get; } = [];
    public string Summary => $"""
        === JUGADORES ===
        Detectados (filas no vacías): {DetectedPlayers}
        Usernames únicos normalizados: {Players.Count}
        Jugadores exclusivos de Victors: {Players.Count(p => p.SourceRows.All(r => r < 0))}
        Nuevos: {Players.Count(p => p.Status == "Nuevo")}
        Ya existentes: {Players.Count(p => p.Status == "Existente")}
        Conflictos (Players): {Players.Count(p => p.Status == "Conflicto")}
        Conflictos de jugador totales (incluye Victors): {Conflicts.Count(c => c.Type == "Jugador")}
        Con AvatarUrl: {Players.Count(p => p.AvatarUrl != null)}
        Sin AvatarUrl: {Players.Count(p => p.AvatarUrl == null)}
        Avatares para jugadores nuevos: {Players.Count(p => p.Status == "Nuevo" && p.AvatarUrl != null)}
        Avatares que podrían actualizarse: {Players.Count(p => p.AvatarAction == "Podría actualizarse")}

        === NIVELES ===
        Filas de Victors: {Levels.Count}
        Resueltos correctamente: {Levels.Count(l => l.Status == "Resuelto")}
        No encontrados: {Levels.Count(l => l.Status == "No encontrado")}
        Ambiguos: {Levels.Count(l => l.Status == "Ambiguo")}
        Otros conflictos: {Levels.Count(l => l.Status == "Conflicto")}

        === RECORDS ===
        Completions detectadas (celdas no vacías): {Records.Count}
        Records nuevos: {Records.Count(r => r.Status == "Nuevo")}
        Records existentes: {Records.Count(r => r.Status == "Existente")}
        Records omitidos: {Records.Count(r => r.Status == "Omitido")}
        Duplicados omitidos: {Records.Count(r => r.Reason == "Combinación Jugador + Nivel repetida en la planilla")}

        === WARNINGS (CONTEOS) ===
        Victor count: {Warnings.Count(w => w.Type == "Victor count")}
        Filas duplicadas: {Warnings.Count(w => w.Type == "Fila de nivel repetida")}
        Victors repetidos: {Warnings.Count(w => w.Type == "Victor repetido")}
        Otros: {Warnings.Count(w => w.Type != "Victor count" && w.Type != "Fila de nivel repetida" && w.Type != "Victor repetido")}
        """;
}
