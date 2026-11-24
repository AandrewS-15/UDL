using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using UDL.Data;
using UDL.Services.Levels;
using UDL.Services.Ranking;
using UDL.ViewModels;

void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
Check(LevelPresentation.Thumbnail(0) is null && LevelPresentation.Thumbnail(-1) is null, "ID inválido genera thumbnail");
Check(LevelPresentation.Thumbnail(76962930) == "https://raw.githubusercontent.com/All-Rated-Extreme-Demon-List/Thumbnails/main/levels/full/76962930.webp", "URL incorrecta");
Check(new PlayerProfileViewModel().Hardest is null, "Hardest de jugador sin records");
Check(new LevelDetailsViewModel().VictorCount == 0, "Victors vacíos");
Check(LevelPresentation.Tags("[\"Wave\",\"2.1\"]").SequenceEqual(new[] { "Wave", "2.1" }), "Tags JSON");
Check(LevelPresentation.Tags(null).Count == 0 && LevelPresentation.Tags("Wave").Single() == "Wave", "Tags vacíos o texto");
Console.WriteLine("OK: thumbnails por GD ID, ID inválido, perfil sin completions y nivel sin victors.");
if (!args.Contains("--database")) return;

const string connection = "Server=localhost;Database=UDL;Integrated Security=True;TrustServerCertificate=True";
using var db = new UdlDbContext(new DbContextOptionsBuilder<UdlDbContext>().UseSqlServer(connection).Options);
var ranking = new PlayerRankingQuery(db);
var profiles = new PlayerProfileService(db, ranking);
var details = new LevelDetailsService(db);
await using var sql = new SqlConnection(connection);
await sql.OpenAsync();
var expectedPlayers = new Dictionary<int, (string Name, decimal Points, int Count, long Rank, string? Avatar)>();
await using (var cmd = new SqlCommand("""
    SELECT j.IdJugador,j.NombreGD,COALESCE(SUM(n.PuntosAredl),0),COUNT(r.IdRecord),
    ROW_NUMBER() OVER(ORDER BY COALESCE(SUM(n.PuntosAredl),0) DESC,COUNT(r.IdRecord) DESC,j.NombreGD),j.AvatarUrl
    FROM dbo.Jugador j LEFT JOIN dbo.Record r ON r.IdJugador=j.IdJugador AND r.Porcentaje=100
    LEFT JOIN dbo.Nivel n ON n.IdNivel=r.IdNivel GROUP BY j.IdJugador,j.NombreGD,j.AvatarUrl
    """, sql))
await using (var reader = await cmd.ExecuteReaderAsync())
    while (await reader.ReadAsync()) expectedPlayers.Add(reader.GetInt32(0), (reader.GetString(1), reader.GetDecimal(2), reader.GetInt32(3), reader.GetInt64(4), reader.IsDBNull(5) ? null : reader.GetString(5)));

foreach (var entry in expectedPlayers)
{
    var p = await profiles.GetAsync(entry.Key) ?? throw new Exception("Perfil ausente");
    var expected = entry.Value;
    Check(p.TotalPuntos == expected.Points && p.CantidadCompletions == expected.Count && p.PosicionUy == expected.Rank && p.AvatarUrl == expected.Avatar, "Perfil no coincide SQL");
    Check(p.Completions.Count == expected.Count && p.Completions.Sum(c => c.PuntosAredl) == expected.Points, "Completions incorrectas");
    await using var cmd = new SqlCommand("SELECT TOP(1) n.IdNivel FROM dbo.Record r JOIN dbo.Nivel n ON n.IdNivel=r.IdNivel WHERE r.IdJugador=@id AND r.Porcentaje=100 ORDER BY n.PosicionAredl,n.Nombre", sql);
    cmd.Parameters.AddWithValue("@id", entry.Key);
    var hardest = await cmd.ExecuteScalarAsync();
    Check(p.Hardest?.IdNivel == (hardest is int value ? value : null), "Hardest incorrecto");
    if (new[] { "Santiricca", "cxve", "Fif" }.Contains(p.NombreGD))
        Console.WriteLine($"PERFIL {p.IdJugador} {p.NombreGD}: #{p.PosicionUy}, {p.TotalPuntos}, {p.CantidadCompletions} completions; hardest {p.Hardest?.Nombre} AREDL #{p.Hardest?.PosicionAredl}; avatar={p.AvatarUrl}");
}
var expectedLevels = new List<(int Id, long Rank, int Count)>();
await using (var cmd = new SqlCommand("""
    SELECT n.IdNivel,ROW_NUMBER() OVER(ORDER BY n.PosicionAredl,n.Nombre),COUNT(r.IdRecord)
    FROM dbo.Nivel n JOIN dbo.Record r ON r.IdNivel=n.IdNivel AND r.Porcentaje=100
    GROUP BY n.IdNivel,n.PosicionAredl,n.Nombre
    """, sql))
await using (var reader = await cmd.ExecuteReaderAsync())
    while (await reader.ReadAsync()) expectedLevels.Add((reader.GetInt32(0), reader.GetInt64(1), reader.GetInt32(2)));
foreach (var expected in expectedLevels)
{
    var l = await details.GetAsync(expected.Id) ?? throw new Exception("Nivel ausente");
    Check(l.PosicionUy == expected.Rank && l.VictorCount == expected.Count, "Nivel no coincide SQL");
    var victorIds = new List<int>();
    await using var cmd = new SqlCommand("SELECT IdJugador FROM dbo.Record WHERE IdNivel=@id AND Porcentaje=100", sql);
    cmd.Parameters.AddWithValue("@id", expected.Id);
    await using (var reader = await cmd.ExecuteReaderAsync())
        while (await reader.ReadAsync()) victorIds.Add(reader.GetInt32(0));
    Check(l.Victors.Select(p => p.IdJugador).Order().SequenceEqual(victorIds.Order()), "Victors incorrectos");
    Check(l.Victors.Select(p => p.TotalPuntos).SequenceEqual(l.Victors.Select(p => p.TotalPuntos).OrderDescending()), "Orden victors incorrecto");
    Check(l.Victors.All(p => p.TotalPuntos == expectedPlayers[p.IdJugador].Points && p.AvatarUrl == expectedPlayers[p.IdJugador].Avatar), "Puntos/avatar victor incorrecto");
    if (new[] { "Sakupen Circles", "Tartarus" }.Contains(l.Nombre))
        Console.WriteLine($"NIVEL {l.IdNivel} {l.Nombre}: UY #{l.PosicionUy}, AREDL #{l.PosicionAredl}, {l.PuntosAredl} pts, {l.VictorCount} victors: {string.Join(", ", l.Victors.Select(p => p.NombreGD))}; thumbnail={l.ThumbnailUrl}");
}
Check(await profiles.GetAsync(int.MaxValue) is null && await details.GetAsync(int.MaxValue) is null, "ID inexistente debe devolver null");
var emptyLevelId = await db.Nivel.Where(n => !n.Record.Any(r => r.Porcentaje == 100)).Select(n => n.IdNivel).FirstAsync();
var emptyLevel = await details.GetAsync(emptyLevelId);
Check(emptyLevel!.VictorCount == 0 && emptyLevel.PosicionUy is null, "Nivel sin completions no tiene posición UY");
Check(!db.ChangeTracker.HasChanges(), "El servicio no debe realizar cambios");
Console.WriteLine($"OK: {expectedPlayers.Count} perfiles y {expectedLevels.Count} detalles contrastados con SQL independiente, hardest, puntos, posiciones, completions, avatares, victors y ausentes. Solo lectura.");
