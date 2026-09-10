using System.Text.Json;
using UDL.Services.Import;

var checks = 0;
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    checks++;
}
var planner = new UdlImportPlanner();
SheetInput Sheet(SheetPlayer[]? players = null, SheetCompletionRow[]? rows = null) =>
    new("https://example.test/sheet", DateTimeOffset.UnixEpoch,
        players ?? [new(2, "Gekura", null)], rows ?? [new(4, "Bloodlust", 253, 1, ["gekura"])]);
var level = new ExistingLevel(7, Guid.NewGuid(), "Bloodlust", 300, false);
var empty = new ImportDatabase(false, [], [level], []);
Check(ImportNames.Username(" Gekura ") == ImportNames.Username("gekura"), "Trim/case username");
Check(ImportNames.Username(" Sunnyagus ") == ImportNames.Username("sunnyagus"), "Username Sunnyagus");
Check(ImportNames.Username("A  B") != ImportNames.Username("A B"), "No fusionar espacios internos de usernames");
Check(ImportNames.Username("User.") != ImportNames.Username("User"), "No borrar puntuación de usernames");
Check(ImportNames.Level("  Bloodlust \t") == ImportNames.Level("Bloodlust"), "Level trim");
Check(ImportNames.Level("Bloodlust") != ImportNames.Level("bloodlust"), "Level case-sensitive");
Check(ImportNames.Level("A  B") == ImportNames.Level("A\u00a0B"), "Espacios de nivel");
Check(ImportNames.Level("cafe\u0301") == ImportNames.Level("café"), "NFC");
Check(ImportNames.Level("cafe") != ImportNames.Level("café"), "Conservar tildes");
Check(ImportNames.Level("BEELINE (Solo)") != ImportNames.Level("BEELINE"), "Conservar Solo");
Check(ImportNames.Level("2\u200e 1\u200e 1") == ImportNames.Level("2 1 1"), "Limpiar caracteres de formato");
var plan = planner.Build(Sheet(), empty);
Check(plan.Players.Single().Status == "Nuevo" && plan.Records.Single().Status == "Nuevo", "Jugador nuevo y nivel único");
Check(plan.Levels.Single().LevelId == 7 && plan.Records.Single().Percentage == 100, "100% e ignorar rank histórico");
var existing = empty with { HasAvatarColumn = true, Players = [new(3, "GEKURA", null)], Records = [new(3, 7, 100)] };
plan = planner.Build(Sheet(), existing);
Check(plan.Players.Single().Status == "Existente" && plan.Records.Single().Status == "Existente", "Jugador/record existente");
Check(planner.Build(Sheet(), existing).Records.All(r => r.Status != "Nuevo"), "Repetición idempotente con estado SQL posterior");
Check(JsonSerializer.Serialize(planner.Build(Sheet(), existing)) == JsonSerializer.Serialize(plan), "Plan determinista");
plan = planner.Build(Sheet(), empty with { Levels = [] });
Check(plan.Levels.Single().Status == "No encontrado" && plan.Records.Single().Status == "Omitido", "Nivel inexistente");
plan = planner.Build(Sheet(), empty with { Levels = [level, level with { Id = 8 }] });
Check(plan.Levels.Single().Status == "Ambiguo" && plan.Conflicts.Single(c => c.Type == "Nivel").Candidates.Count == 2, "Nivel ambiguo");
plan = planner.Build(Sheet(), existing with { Players = [new(3, "GEKURA", null), new(4, "gekura", null)] });
Check(plan.Players.Single().Status == "Conflicto" && plan.Records.Single().Status == "Omitido", "Jugador SQL ambiguo");
plan = planner.Build(Sheet([new(2, "Gekura", null), new(3, "gekura", null)],
    [new(4, "Bloodlust", 253, 2, ["Gekura", "gekura"]), new(5, "Bloodlust", 253, 1, ["GEKURA"])]), empty);
Check(plan.Players.Count == 1 && plan.Records.Count(r => r.Status == "Nuevo") == 1
    && plan.Records.Count(r => r.Status == "Omitido") == 2, "Duplicados en Players, fila y entre filas");
Check(plan.Warnings.Any(w => w.Type == "Victor repetido"), "Warning duplicado");
plan = planner.Build(Sheet(rows: [new(4, "Bloodlust", 253, 7, ["gekura"])]), empty);
Check(plan.Warnings.Any(w => w.Type == "Victor count"), "Victor count incorrecto");
plan = planner.Build(Sheet(rows: [new(4, "Bloodlust", 253, 1, ["Gekura."])]), empty);
Check(plan.Records.Single().Status == "Nuevo" && plan.Records.Single().CanonicalUsername == "Gekura", "Punto final de Victor y nombre canónico");
plan = planner.Build(Sheet(rows: [new(4, "Bloodlust", 253, 1, ["Unknown"])]), empty);
Check(plan.Conflicts.Count == 0 && plan.Players.Count == 2 && plan.Players.Single(p => p.Username == "Unknown").AvatarUrl == null,
    "Victor válido ausente crea jugador histórico sin avatar");
plan = planner.Build(Sheet(rows: [new(4, "Bloodlust (Solo)", 253, 1, ["gekura"])]), empty);
Check(plan.Levels.Single().Status == "No encontrado" && plan.Levels.Single().Candidates.Count == 1, "Solo no vincula al nivel base");
plan = planner.Build(Sheet(), empty with { Levels = [level with { TwoPlayer = true }] });
Check(plan.Levels.Single().Status == "Conflicto", "2-player requiere modalidad explícita");
plan = planner.Build(Sheet([new(2, "Gekura", "https://example.test/a.png")]), existing);
Check(plan.Players.Single().AvatarAction == "Podría actualizarse", "Avatar faltante existente");
plan = planner.Build(Sheet([new(2, "Gekura", "https://example.test/a.png")]),
    existing with { Players = [new(3, "gekura", "https://example.test/b.png")] });
Check(plan.Players.Single().AvatarAction == "Conservar existente" && plan.Warnings.Any(w => w.Type == "Avatar existente"), "No sobrescribir avatar");
foreach (var bad in new[] { "javascript:alert(1)", "file:///a.png", "https://user:pass@example.test/a", "https://example.test/" + new string('a', 501) })
{
    plan = planner.Build(Sheet([new(2, "Gekura", bad)]), empty);
    Check(plan.Players.Single().AvatarUrl == null && plan.Warnings.Any(w => w.Type == "Avatar inválido"), "Avatar inválido");
}
plan = planner.Build(Sheet([new(2, "Gekura", "https://example.test/a"), new(3, "gekura", "https://example.test/b")]), empty);
Check(plan.Players.Single().Status == "Conflicto", "Avatares contradictorios");
plan = planner.Build(Sheet(), existing with { Records = [new(3, 7, 80)] });
Check(plan.Records.Single().Status == "Existente" && plan.Warnings.Any(w => w.Type == "Record existente parcial"), "No reemplazar record parcial");

const string json = """
{"SourceUrl":"https://example.test", "CapturedAtUtc":"2026-09-10T00:00:00Z",
"PlayersRange":"Players!A1:BH994", "VictorsRange":"Victors!A1:BO312",
"Players":[["username","avatar URL","points"],[" Gekura ","",99999],[" ","",1]],
"Victors":[[],["aredl rank","rank","","points","victor count"],["","","Level Name"],
[253,"#1","Bloodlust",99999,2,"gekura","","GEKURA"],["","#2"]]}
""";
var parsed = JsonSerializer.Deserialize<UdlSheetSnapshot>(json)!.Parse();
Check(parsed.Players.Count == 1 && parsed.Players[0].Username == "Gekura" && parsed.Players[0].AvatarUrl == null, "Parser Players y vacíos");
Check(parsed.Victors.Count == 1 && parsed.Victors[0].Victors.Count == 2, "Parser columnas dispersas y filas de plantilla");
Check(planner.Build(parsed, empty).Records.Count(r => r.Status == "Nuevo") == 1, "Parser ignora puntos y deduplica");
try { JsonSerializer.Deserialize<UdlSheetSnapshot>(json.Replace("username", "wrong"))!.Parse(); throw new Exception("Aceptó encabezado incorrecto"); }
catch (InvalidDataException) { checks++; }
Check(ImportNames.Victor(" Thal\u200bion087. ") == ImportNames.Username("Thalion087"), "Jugador: formato y punto final");
Check(ImportNames.Victor("A.B.") == ImportNames.Username("A.B"), "Conservar punto interno");
Check(ImportNames.Victor("A..") == ImportNames.Username("A."), "Quitar un solo punto final");
plan = planner.Build(Sheet([new(2, "Thalion087", null)],
    [new(4, "Bloodlust", 253, 2, ["Thalion087.", "THALION087"])]), empty);
Check(plan.Records.First().CanonicalUsername == "Thalion087" && plan.Records.Count(r => r.Status == "Nuevo") == 1,
    "Thalion087 canónico y deduplicación tras normalizar");
Check(ImportNames.Level("Erebus") != ImportNames.Level("ErebuS"), "Erebus != ErebuS");
var bold = level with { Id = 20, Name = "Erebus (BoldStep)" };
var plat = level with { Id = 21, Name = "ErebuS (Platnuu)" };
plan = planner.Build(Sheet(rows: [new(4, "Erebus", 677, 1, ["gekura"])]), empty with { Levels = [plat, bold] });
Check(plan.Levels.Single().LevelId == 20 && plan.Levels.Single().Resolution!.StartsWith("Alias"), "Erebus alias BoldStep, ignorando rank");
plan = planner.Build(Sheet(rows: [new(4, "Erebus", 677, 1, ["gekura"])]),
    empty with { Levels = [plat with { Name = "ErebuS" }, bold with { Name = "Erebus" }] });
Check(plan.Levels.Single().LevelId == 20 && plan.Levels.Single().Resolution == "Exacta", "Nombres diferenciados por case");
var heartbeat = level with { Id = 22, Name = "Heartbeat (KrmaL)" };
plan = planner.Build(Sheet(rows: [new(4, "Heartbeat", 1070, 1, ["gekura"])]), empty with { Levels = [heartbeat] });
Check(plan.Levels.Single().LevelId == 22 && plan.Levels.Single().Resolution!.StartsWith("Alias"), "Alias Heartbeat");
plan = planner.Build(Sheet(rows: [new(4, "Heartbeat", 1070, 1, ["gekura"])]),
    empty with { Levels = [heartbeat, level with { Id = 23, Name = "Heartbeat" }] });
Check(plan.Levels.Single().LevelId == 23 && plan.Levels.Single().Resolution == "Exacta", "Exacta tiene prioridad sobre alias");
plan = planner.Build(Sheet(rows: [new(4, "Heartbeat", 1070, 1, ["gekura"])]),
    empty with { Levels = [heartbeat, heartbeat with { Id = 24 }] });
Check(plan.Levels.Single().Status == "Ambiguo", "Alias ambiguo no vincula");
plan = planner.Build(Sheet(rows: [new(4, "2\u200e 1\u200e 1", 539, 1, ["gekura"])]),
    empty with { Levels = [level with { Name = "2 1 1" }] });
Check(plan.Levels.Single().LevelId == 7 && plan.Levels.Single().Resolution == "Exacta", "2 1 1 exacto tras limpiar formato");
foreach (var typo in new[] { "Bloodlus", "heartbeat", "ErebuS" })
{
    plan = planner.Build(Sheet(rows: [new(4, typo, 300, 1, ["gekura"])]), empty);
    Check(plan.Levels.Single().Status == "No encontrado" && plan.Records.Single().Status == "Omitido", "Sin fuzzy ni fallback de rank");
}
plan = planner.Build(Sheet(rows: [new(4, "Kowareta", 300, 1, ["gekura"])]),
    empty with { Levels = [level with { Name = "kowareta" }] });
Check(plan.Levels.Single().LevelId == 7 && plan.Levels.Single().Resolution == "Case-insensitive única", "Kowareta único ignorando case");
plan = planner.Build(Sheet(rows: [new(4, "EREBUS", 300, 1, ["gekura"])]),
    empty with { Levels = [bold with { Name = "Erebus" }, plat with { Name = "ErebuS" }] });
Check(plan.Levels.Single().Status == "Ambiguo" && plan.Records.Single().Status == "Omitido", "Case-insensitive ambiguo");
plan = planner.Build(Sheet(rows: [new(4, "Heartbeat", 300, 1, ["gekura"])]),
    empty with { Levels = [heartbeat, level with { Name = "heartbeat" }, level with { Id = 25, Name = "HEARTBEAT" }] });
Check(plan.Levels.Single().Status == "Ambiguo", "Un alias no oculta ambigüedad case-insensitive");
plan = planner.Build(Sheet(rows: [new(4, "Heartbeat", 300, 1, ["gekura"])]),
    empty with { Levels = [heartbeat, level with { Name = "heartbeat" }] });
Check(plan.Levels.Single().LevelId == 7, "Case-insensitive único precede al alias");
Check(UdlSheetSnapshot.AvatarValue("=IMAGE(\"https://example.test/a.png\",1)") == "https://example.test/a.png", "URL literal IMAGE");
Check(UdlSheetSnapshot.AvatarValue("=IMAGE(A2,1)") == "=IMAGE(A2,1)", "No evaluar fórmulas de avatar");
Check(UdlSheetSnapshot.AvatarValue("  ") == null, "Avatar realmente vacío");
plan = planner.Build(Sheet([new(2, "Okarun", null)], [new(4, "Bloodlust", 253, 1, ["Okrun"])]), empty);
Check(plan.Records.Single().Status == "Nuevo" && plan.Records.Single().CanonicalUsername == "Okarun"
    && plan.Players.Count == 1 && plan.Conflicts.Count == 0, "Alias manual Okrun a Okarun");
plan = planner.Build(Sheet(rows: [new(4, "Bloodlust", 253, 3, ["ThaLion087", "thalion087", "ThaLion087."])]), empty);
Check(plan.Players.Count(p => p.Key == "THALION087") == 1 && plan.Players.Single(p => p.Key == "THALION087").Username == "ThaLion087"
    && plan.Records.Count(r => r.Status == "Nuevo") == 1, "Jugador Victors canónico único y records sin duplicados");
plan = planner.Build(Sheet(rows: [new(4, "Bloodlust", 253, 1, [new string('a', 51)])]), empty);
Check(plan.Conflicts.Any(c => c.Type == "Jugador"), "Nombre Victors inválido no importable");
plan = planner.Build(Sheet(rows: [new(4, "Bloodlust", 253, 1, ["ThaLion087"])]),
    empty with { Players = [new(9, "THALION087", null)], Records = [new(9, 7, 100)] });
Check(plan.Records.Single().Status == "Existente" && plan.Players.Single(p => p.Key == "THALION087").Status == "Existente",
    "Histórico solo Victors idempotente después de importar");
Console.WriteLine($"OK: {checks} comprobaciones de importación (normalización, resolución, aliases, conflictos, duplicados, idempotencia, avatares y parser).");
