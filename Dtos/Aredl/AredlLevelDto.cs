using System.Text.Json.Serialization;

namespace UDL.Dtos.Aredl;

// Contrato externo de AREDL; no es una entidad de Entity Framework.
public sealed class AredlLevelDto
{
    [JsonPropertyName("id")]
    public required Guid Id { get; init; }
    [JsonPropertyName("name")]
    public required string Name { get; init; }
    [JsonPropertyName("position")]
    public int? Position { get; init; }
    [JsonPropertyName("publisher_id")]
    public required Guid PublisherId { get; init; }
    [JsonPropertyName("points")]
    public required int Points { get; init; }
    // Se conserva el texto para tolerar futuros estados sin perder información.
    [JsonPropertyName("status")]
    public required string Status { get; init; }
    [JsonPropertyName("requires_raw_footage")]
    public required bool RequiresRawFootage { get; init; }
    [JsonPropertyName("level_id")]
    public required int LevelId { get; init; }
    [JsonPropertyName("two_player")]
    public required bool TwoPlayer { get; init; }
    [JsonPropertyName("tags")]
    public required List<string?> Tags { get; init; }
    [JsonPropertyName("description")]
    public string? Description { get; init; }
    [JsonPropertyName("song")]
    public int? Song { get; init; }
    [JsonPropertyName("edel_enjoyment")]
    public double? EdelEnjoyment { get; init; }
    [JsonPropertyName("is_edel_pending")]
    public required bool IsEdelPending { get; init; }
    [JsonPropertyName("gddl_tier")]
    public double? GddlTier { get; init; }
    [JsonPropertyName("nlw_tier")]
    public string? NlwTier { get; init; }
    [JsonPropertyName("completed_by_user")]
    public bool? CompletedByUser { get; init; }
}
