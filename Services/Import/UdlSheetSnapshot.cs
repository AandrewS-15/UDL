using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace UDL.Services.Import;

public sealed record SheetPlayer(int Row, string Username, string? AvatarUrl);
public sealed record SheetCompletionRow(int Row, string LevelName, int? HistoricalRank,
    int? VictorCount, IReadOnlyList<string> Victors);
public sealed record SheetInput(string SourceUrl, DateTimeOffset CapturedAtUtc,
    IReadOnlyList<SheetPlayer> Players, IReadOnlyList<SheetCompletionRow> Victors);

/// <summary>Snapshot completo de valores de Players y Victors obtenido con Google Sheets.
/// Los puntos, rankings y challenges no forman parte del modelo de importación.</summary>
public sealed class UdlSheetSnapshot
{
    public required string SourceUrl { get; init; }
    public required DateTimeOffset CapturedAtUtc { get; init; }
    public required string PlayersRange { get; init; }
    public required string VictorsRange { get; init; }
    public required JsonElement[][] Players { get; init; }
    public required JsonElement[][] Victors { get; init; }

    public static async Task<SheetInput> ReadAsync(string path, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(path);
        var snapshot = await JsonSerializer.DeserializeAsync<UdlSheetSnapshot>(stream, cancellationToken: ct)
            ?? throw new InvalidDataException("Snapshot vacío.");
        return snapshot.Parse();
    }

    public SheetInput Parse()
    {
        static string Cell(JsonElement[] row, int col) => col >= row.Length ? "" :
            row[col].ValueKind == JsonValueKind.String ? row[col].GetString() ?? "" :
            row[col].ValueKind is JsonValueKind.Null ? "" : row[col].ToString();
        static int? Integer(string value) => int.TryParse(value.Trim(), NumberStyles.Integer,
            CultureInfo.InvariantCulture, out var n) && n >= 0 ? n : null;

        if (Players.Length == 0 || Victors.Length < 3
            || ImportNames.Username(Cell(Players[0], 0)) != "USERNAME"
            || ImportNames.Username(Cell(Players[0], 1)) != "AVATAR URL"
            || ImportNames.Username(Cell(Victors[1], 0)) != "AREDL RANK"
            || ImportNames.Username(Cell(Victors[1], 4)) != "VICTOR COUNT"
            || ImportNames.Username(Cell(Victors[2], 2)) != "LEVEL NAME")
            throw new InvalidDataException("Encabezados incompatibles: se esperaba Players A=username/B=avatar URL y Victors A=aredl rank/C=Level Name/E=victor count/F+=victors.");

        var players = Players.Skip(1).Select((r, i) => new SheetPlayer(i + 2,
            Cell(r, 0).Trim(), AvatarValue(Cell(r, 1))))
            .Where(p => p.Username.Length > 0).ToArray();
        var rows = new List<SheetCompletionRow>();
        for (var i = 3; i < Victors.Length; i++)
        {
            var r = Victors[i];
            var name = Cell(r, 2).Trim();
            var victors = r.Skip(5).Select((_, j) => Cell(r, j + 5).Trim())
                .Where(v => v.Length > 0).ToArray();
            // Las filas de plantilla con solo UY rank no representan niveles.
            if (name.Length == 0 && victors.Length == 0 && string.IsNullOrWhiteSpace(Cell(r, 0))
                && string.IsNullOrWhiteSpace(Cell(r, 4))) continue;
            rows.Add(new(i + 1, name, Integer(Cell(r, 0)), Integer(Cell(r, 4)), victors));
        }
        return new(SourceUrl, CapturedAtUtc, players, rows);
    }

    // IMAGE no tiene un valor textual efectivo. Extraer únicamente URL literal,
    // sin evaluar fórmulas ni descargar imágenes. Otras fórmulas siguen como valor
    // inválido para que el planner produzca warning, nunca como celda vacía.
    public static string? AvatarValue(string value)
    {
        value = value.Trim();
        if (value.Length == 0) return null;
        var match = Regex.Match(value, """^=IMAGE\(\s*"((?:[^"]|"")*)"\s*(?:[,;]\s*[1-4]\s*)?\)$""",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success ? match.Groups[1].Value.Replace("\"\"", "\"") : value;
    }
}
