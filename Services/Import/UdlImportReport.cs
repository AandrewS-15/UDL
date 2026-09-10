using System.Text;
using System.Text.Json;

namespace UDL.Services.Import;

public static class UdlImportReport
{
    public static string Render(ImportPlan plan, bool applied = false)
    {
        var text = new StringBuilder();
        text.AppendLine(applied ? "IMPORTACIÓN REAL CONFIRMADA — transacción completada; contadores describen las acciones de esta ejecución."
            : "DRY RUN — solo lectura SQL; no se ejecutó ninguna inserción, actualización ni ALTER.");
        text.AppendLine($"Fuente: {plan.SourceUrl}");
        text.AppendLine($"Captura UTC: {plan.CapturedAtUtc:O}");
        text.AppendLine(plan.Summary);
        text.AppendLine("\n=== CONFLICTOS ===");
        foreach (var c in plan.Conflicts)
        {
            text.AppendLine($"[{c.Type}] fila {c.Row}: {c.Name} | AREDL histórico: {c.HistoricalRank?.ToString() ?? "—"} | {c.Reason}");
            text.AppendLine("  Posibles coincidencias: " + (c.Candidates.Count == 0 ? "ninguna" : string.Join("; ", c.Candidates)));
            if (c.AppearsInPlayers is bool present)
            {
                text.AppendLine($"  Aparece en Players (matching de username): {(present ? "sí" : "no")}");
                text.AppendLine("  Candidatos Players (solo revisión, no vinculados): " +
                    (c.PlayersCandidates?.Count > 0 ? string.Join("; ", c.PlayersCandidates) : "ninguno"));
                text.AppendLine("  Candidatos SQL: " +
                    (c.SqlCandidates?.Count > 0 ? string.Join("; ", c.SqlCandidates) : "ninguno"));
            }
        }
        if (plan.Conflicts.Count == 0) text.AppendLine("Ninguno.");
        text.AppendLine("\n=== WARNINGS ===");
        foreach (var w in plan.Warnings) text.AppendLine($"[{w.Type}] fila {w.Row}: {w.Name} | {w.Message}");
        if (plan.Warnings.Count == 0) text.AppendLine("Ninguno.");
        text.AppendLine("\n=== DETALLE JUGADORES ===");
        foreach (var p in plan.Players)
            text.AppendLine($"{p.Username} | {p.Status} | IdJugador={p.ExistingId?.ToString() ?? "nuevo"} | Avatar: {p.AvatarAction} | URL propuesta={p.AvatarUrl ?? "NULL"}");
        text.AppendLine("\n=== DETALLE RECORDS (100%) ===");
        foreach (var r in plan.Records)
            text.AppendLine($"Victors fila {r.Row} | {r.Username} + {r.LevelName} | Canónico={r.CanonicalUsername ?? "sin resolver"} | IdNivel={r.LevelId?.ToString() ?? "sin resolver"} | {r.Status} | {r.Reason}");
        return text.ToString();
    }

    public static async Task WriteAsync(ImportPlan plan, string prefix, CancellationToken ct = default, bool applied = false)
    {
        var path = Path.GetFullPath(prefix);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await File.WriteAllTextAsync(path + ".txt", Render(plan, applied), Encoding.UTF8, ct);
        await File.WriteAllTextAsync(path + ".json", JsonSerializer.Serialize(plan,
            new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8, ct);
    }
}
