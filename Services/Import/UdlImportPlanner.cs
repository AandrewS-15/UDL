namespace UDL.Services.Import;

/// <summary>Plan puro: no depende de EF, HTTP ni escritura en la base de datos.</summary>
public sealed class UdlImportPlanner
{
    public ImportPlan Build(SheetInput sheet, ImportDatabase db)
    {
        var plan = new ImportPlan { SourceUrl = sheet.SourceUrl, CapturedAtUtc = sheet.CapturedAtUtc,
            HasAvatarColumn = db.HasAvatarColumn, DetectedPlayers = sheet.Players.Count };
        if (!db.HasAvatarColumn)
            plan.Warnings.Add(new("Schema", 0, "Jugador.AvatarUrl", "Columna ausente; se considera NULL para este dry run. El ALTER pendiente no se ejecutó."));
        var sqlPlayers = db.Players.ToLookup(p => ImportNames.Username(p.Username));
        var playerSources = sheet.Players.ToList();
        var known = playerSources.Select(p => ImportNames.Username(p.Username)).ToHashSet();
        foreach (var row in sheet.Victors)
            foreach (var name in row.Victors)
            {
                var canonical = UdlPlayerAliases.Canonical(name);
                if (known.Contains(ImportNames.Victor(canonical))) continue;
                if (known.Add(ImportNames.Username(canonical)))
                    playerSources.Add(new(-row.Row, canonical, null)); // fila negativa identifica origen Victors
            }
        foreach (var group in playerSources.GroupBy(p => ImportNames.Username(p.Username)))
        {
            var first = group.First();
            var existing = sqlPlayers[group.Key].ToArray();
            var avatars = group.Select(p => p.AvatarUrl?.Trim()).Where(a => !string.IsNullOrEmpty(a))
                .Distinct(StringComparer.Ordinal).ToArray();
            var reason = first.Username.Length > 50 ? "Username excede NVARCHAR(50)" :
                group.Key.Length == 0 || first.Username.Any(char.IsControl) ? "Username vacío o con caracteres de control" :
                existing.Length > 1 ? "Múltiples jugadores SQL coinciden con el username normalizado" :
                avatars.Length > 1 ? "Filas del mismo jugador contienen avatares diferentes" : null;
            if (group.Count() > 1)
                plan.Warnings.Add(new("Jugador duplicado", first.Row, first.Username,
                    $"Se agrupan filas Players {string.Join(", ", group.Select(p => p.Row))} sin distinguir mayúsculas/minúsculas."));
            var avatar = avatars.FirstOrDefault();
            if (avatar != null && !ValidAvatar(avatar))
            {
                plan.Warnings.Add(new("Avatar inválido", first.Row, first.Username,
                    "Avatar omitido: se requiere URL absoluta HTTP(S), sin credenciales, de hasta 500 caracteres."));
                avatar = null;
            }
            var currentAvatar = existing.SingleOrDefaultIfUnique()?.AvatarUrl;
            var avatarAction = avatar == null ? "Sin avatar de origen" : existing.Length == 0 ? "Para insertar" :
                string.IsNullOrWhiteSpace(currentAvatar) ? "Podría actualizarse" : "Conservar existente";
            if (avatar != null && !string.IsNullOrWhiteSpace(currentAvatar) && avatar != currentAvatar)
                plan.Warnings.Add(new("Avatar existente", first.Row, first.Username,
                    "SQL ya tiene un avatar distinto: se conserva y no se sobrescribe."));
            plan.Players.Add(new(ImportNames.Clean(first.Username), group.Key, reason != null ? "Conflicto" :
                existing.Length == 1 ? "Existente" : "Nuevo", existing.Length == 1 ? existing[0].Id : null,
                avatar, avatarAction, group.Select(p => p.Row).ToArray()));
            if (reason != null)
                plan.Conflicts.Add(new("Jugador", first.Row, first.Username, null, reason,
                    existing.Select(p => $"SQL IdJugador={p.Id}: {p.Username}").ToArray()));
        }

        var players = plan.Players.ToDictionary(p => p.Key);
        var levels = db.Levels.ToLookup(l => ImportNames.Level(l.Name));
        var levelsIgnoringCase = db.Levels.ToLookup(l => ImportNames.Level(l.Name), StringComparer.OrdinalIgnoreCase);
        var records = db.Records.ToDictionary(r => (r.PlayerId, r.LevelId));
        var seen = new HashSet<(string Player, int Level)>();
        var seenLevelRows = new HashSet<string>();
        foreach (var row in sheet.Victors)
        {
            if (row.VictorCount != row.Victors.Count)
                plan.Warnings.Add(new("Victor count", row.Row, row.LevelName,
                    $"Declarado: {row.VictorCount?.ToString() ?? "inválido/vacío"}; nombres no vacíos: {row.Victors.Count}. Revisar manualmente."));
            if (row.HistoricalRank is null or <= 0)
                plan.Warnings.Add(new("Rank histórico", row.Row, row.LevelName, "Rank histórico ausente o inválido; nunca se utiliza para vincular."));
            foreach (var repeated in row.Victors.GroupBy(ImportNames.Victor).Where(g => g.Count() > 1))
                plan.Warnings.Add(new("Victor repetido", row.Row, row.LevelName,
                    $"{repeated.First()} aparece {repeated.Count()} veces; no genera records duplicados."));
            if (!seenLevelRows.Add(ImportNames.Level(row.LevelName)))
                plan.Warnings.Add(new("Fila de nivel repetida", row.Row, row.LevelName, "Nivel repetido en Victors; se deduplican las combinaciones resueltas."));
            var matches = levels[ImportNames.Level(row.LevelName)].ToArray();
            var resolution = "Exacta";
            if (matches.Length == 0)
            {
                matches = levelsIgnoringCase[ImportNames.Level(row.LevelName)].ToArray();
                resolution = "Case-insensitive única";
            }
            // Un resultado ambiguo (exacto o case-insensitive) nunca se sustituye por un alias.
            if (matches.Length == 0 && UdlLevelAliases.Target(ImportNames.Level(row.LevelName)) is string alias)
            {
                matches = levels[ImportNames.Level(alias)].ToArray();
                resolution = $"Alias explícito: {alias}";
            }
            string? reason = null;
            var status = "Resuelto";
            if (string.IsNullOrWhiteSpace(ImportNames.Level(row.LevelName))) { status = "Conflicto"; reason = "Fila sin Level Name"; }
            else if (matches.Length == 0) { status = "No encontrado"; reason = "Sin coincidencia exacta, case-insensitive única ni alias explícito"; }
            else if (matches.Length > 1) { status = "Ambiguo"; reason = $"Múltiples niveles coinciden en búsqueda {resolution}; requiere revisión manual"; }
            else
            {
                var key = ImportNames.Level(row.LevelName);
                var explicitTwoPlayer = key.EndsWith("(2P)") || key.EndsWith("(2-PLAYER)");
                if (matches[0].TwoPlayer != explicitTwoPlayer)
                { status = "Conflicto"; reason = "Modalidad 2-player de SQL no coincide con el calificador explícito de la planilla"; }
            }
            var candidates = matches.Length > 0 ? matches : db.Levels.Where(l =>
                ImportNames.Suggestion(l.Name) == ImportNames.Suggestion(row.LevelName)
                || l.Position == row.HistoricalRank && row.HistoricalRank is > 0).ToArray();
            var levelId = status == "Resuelto" ? matches[0].Id : (int?)null;
            plan.Levels.Add(new(row.Row, row.LevelName, row.HistoricalRank, status, levelId, candidates,
                status == "Resuelto" ? resolution : null));
            if (reason != null)
                plan.Conflicts.Add(new("Nivel", row.Row, row.LevelName, row.HistoricalRank, reason,
                    candidates.Select(l => $"SQL IdNivel={l.Id}, AredlId={l.AredlId}, nombre={l.Name}, posición actual={l.Position}, TwoPlayer={l.TwoPlayer}, GeometryDashId={l.GeometryDashId}, Publisher={l.Publisher ?? "desconocido"}").ToArray()));

            foreach (var victor in row.Victors)
            {
                var canonicalVictor = UdlPlayerAliases.Canonical(victor);
                var key = ImportNames.Victor(canonicalVictor);
                if (!players.ContainsKey(key) && players.ContainsKey(ImportNames.Username(canonicalVictor)))
                    key = ImportNames.Username(canonicalVictor);
                players.TryGetValue(key, out var player);
                // Players conserva prioridad; nombres exclusivos de Victors se planifican arriba.
                var sqlMatches = sqlPlayers[key].ToArray();
                var playerId = player?.ExistingId ?? (sqlMatches.Length == 1 ? sqlMatches[0].Id : (int?)null);
                var playerReason = player?.Status == "Conflicto" ? "Jugador tiene conflictos en Players" :
                    player == null && sqlMatches.Length != 1 ? "Victor no presente en Players ni resuelto de forma única en SQL" : null;
                if (playerReason != null)
                {
                    var sqlSuggestions = db.Players.Where(p => ReviewCandidate(victor, p.Username))
                        .Select(p => $"SQL IdJugador={p.Id}: {p.Username}").ToArray();
                    var sheetSuggestions = plan.Players.Where(p => ReviewCandidate(victor, p.Username))
                        .Select(p => p.Username).ToArray();
                    var suggestions = sqlSuggestions.Concat(sheetSuggestions.Select(n => $"Players: {n} (solo revisión)")).ToArray();
                    plan.Conflicts.Add(new("Jugador", row.Row, victor, row.HistoricalRank,
                        $"{playerReason} (nivel: {row.LevelName})", suggestions, player != null,
                        sheetSuggestions, sqlSuggestions));
                }
                var recordStatus = "Nuevo";
                var recordReason = playerReason ?? reason;
                if (recordReason != null) recordStatus = "Omitido";
                else if (!seen.Add((playerId is int id ? $"SQL:{id}" : $"NEW:{key}", levelId!.Value)))
                { recordStatus = "Omitido"; recordReason = "Combinación Jugador + Nivel repetida en la planilla"; }
                else if (playerId is int existingId && records.TryGetValue((existingId, levelId!.Value), out var record))
                {
                    recordStatus = "Existente";
                    if (record.Percentage != 100)
                        plan.Warnings.Add(new("Record existente parcial", row.Row, victor,
                            $"SQL ya tiene {record.Percentage}% para {row.LevelName}; no se modifica ni se crea otro record."));
                }
                plan.Records.Add(new(row.Row, victor, row.LevelName, levelId, 100, recordStatus, recordReason,
                    player?.Username ?? (sqlMatches.Length == 1 ? sqlMatches[0].Username : null)));
            }
        }
        return plan;
    }

    private static bool ValidAvatar(string value) => value.Length <= 500 &&
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https"
        && !string.IsNullOrWhiteSpace(uri.Host) && string.IsNullOrEmpty(uri.UserInfo);

    // Solo informe de jugadores no resueltos, nunca matching: sugerir como máximo
    // una inserción, eliminación o sustitución para nombres de al menos 4 caracteres.
    private static bool ReviewCandidate(string source, string candidate)
    {
        var a = ImportNames.Victor(source);
        var b = ImportNames.Username(candidate);
        if (a == b) return true;
        if (Math.Min(a.Length, b.Length) < 4 || Math.Abs(a.Length - b.Length) > 1) return false;
        var i = 0; var j = 0; var differences = 0;
        while (i < a.Length && j < b.Length)
        {
            if (a[i] == b[j]) { i++; j++; continue; }
            if (++differences > 1) return false;
            if (a.Length >= b.Length) i++;
            if (b.Length >= a.Length) j++;
        }
        return differences + (a.Length - i) + (b.Length - j) <= 1;
    }
}

internal static class UniqueMatch
{
    public static T? SingleOrDefaultIfUnique<T>(this T[] values) where T : class => values.Length == 1 ? values[0] : null;
}
