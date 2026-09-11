using Microsoft.EntityFrameworkCore;
using UDL.Data;
using UDL.Models;
using UDL.Services.Ranking;

var players = new[] {
    new Jugador { IdJugador = 1, NombreGd = "Zeta" },
    new Jugador { IdJugador = 2, NombreGd = "Beta", AvatarUrl = "https://example.test/avatar.png" },
    new Jugador { IdJugador = 3, NombreGd = "Alpha" },
    new Jugador { IdJugador = 4, NombreGd = "Sin records" }
};
Record R(int player, byte percent, decimal points) => new() {
    IdJugador = player, Porcentaje = percent, IdNivelNavigation = new Nivel { PuntosAredl = points }
};
var records = new[] { R(1, 100, 30.1m), R(2, 100, 10m), R(2, 100, 20m),
    R(3, 100, 30m), R(4, 99, 10000m) };
var result = PlayerRankingQuery.Build(players.AsQueryable(), records.AsQueryable()).ToArray();
if (!result.Select(p => p.IdJugador).SequenceEqual(new[] { 1, 2, 3, 4 })) throw new Exception("Orden por puntos/completions incorrecto");
if (result[0].TotalPuntos != 30.1m || result[1].CantidadCompletions != 2
    || result[3].TotalPuntos != 0 || result[3].CantidadCompletions != 0) throw new Exception("Suma, conteo o exclusión de progresos incorrectos");
if (result[1].AvatarUrl != players[1].AvatarUrl || result[3].AvatarUrl != null) throw new Exception("Avatares incorrectos");
var tied = PlayerRankingQuery.Build(players.AsQueryable(), new[] { R(2, 100, 10), R(3, 100, 10) }.AsQueryable()).ToArray();
if (tied[0].NombreGD != "Alpha" || tied[1].NombreGD != "Beta") throw new Exception("Desempate alfabético incorrecto");
using var db = new UdlDbContext(new DbContextOptionsBuilder<UdlDbContext>().UseSqlServer("Server=localhost;Database=UDL;Integrated Security=True;TrustServerCertificate=True").Options);
var sql = PlayerRankingQuery.Build(db.Jugador.AsNoTracking(), db.Record.AsNoTracking()).ToQueryString();
if (!sql.Contains("SUM(") || !sql.Contains("COUNT(")) throw new Exception("No se tradujo la agregación a SQL");
Console.WriteLine("OK: sumas decimales, solo 100%, jugadores sin records, desempates, avatares y traducción SQL sin acceso a la base.");
Console.WriteLine(sql);
