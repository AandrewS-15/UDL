using System.Globalization;
using System.Text.Json;

namespace UDL.Services.Levels;

public static class LevelPresentation
{
    public static IReadOnlyList<string> Tags(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];
        try
        {
            return (JsonSerializer.Deserialize<string[]>(value) ?? [])
                .Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
        }
        catch (JsonException) { return [value]; }
    }

    public static string Category(int? position) => position switch
    {
        >= 1 and <= 75 => "Main List",
        >= 76 and <= 150 => "Extended",
        >= 151 => "Legacy",
        _ => "Sin posición"
    };

    // AREDL Thumbnails/src/image/levels.ts: levels/full/{level.level_id}.webp.
    public static string? Thumbnail(int geometryDashId) => geometryDashId > 0
        ? "https://raw.githubusercontent.com/All-Rated-Extreme-Demon-List/Thumbnails/main/levels/full/"
            + geometryDashId.ToString(CultureInfo.InvariantCulture) + ".webp"
        : null;
}
