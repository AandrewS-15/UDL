using Microsoft.EntityFrameworkCore;
using UDL.Data;
using UDL.Models;
using UDL.Services.Levels;
using UDL.ViewModels;

var levels = new[] {
    new Nivel { IdNivel = 1, Nombre = "Sin records", PosicionAredl = 1, PuntosAredl = 10000 },
    new Nivel { IdNivel = 2, Nombre = "Solo progreso", PosicionAredl = 2, PuntosAredl = 9000 },
    new Nivel { IdNivel = 3, Nombre = "Beta", PosicionAredl = 43, PuntosAredl = 4519.3m },
    new Nivel { IdNivel = 4, Nombre = "Alpha", PosicionAredl = 43, PuntosAredl = 4519.3m },
    new Nivel { IdNivel = 5, Nombre = "Legacy", PosicionAredl = 253, PuntosAredl = 613m }
};
var records = new[] {
    new Record { IdNivel = 2, Porcentaje = 99 },
    new Record { IdNivel = 3, IdJugador = 1, Porcentaje = 100 },
    new Record { IdNivel = 3, IdJugador = 2, Porcentaje = 100 },
    new Record { IdNivel = 3, IdJugador = 3, Porcentaje = 80 },
    new Record { IdNivel = 4, Porcentaje = 100 },
    new Record { IdNivel = 5, Porcentaje = 100 }
};
var result = UruguayLevelListQuery.Build(levels.AsQueryable(), records.AsQueryable()).ToArray();
if (!result.Select(l => l.IdNivel).SequenceEqual(new[] { 4, 3, 5 })) throw new Exception("Inclusión u orden incorrecto");
if (result[1].VictorCount != 2 || result[1].PuntosAredl != 4519.3m || result[2].PuntosAredl != 613m)
    throw new Exception("Conteo o lectura de puntos incorrectos");
if (UruguayLevelListQuery.Build(levels.AsQueryable(), Array.Empty<Record>().AsQueryable()).Any())
    throw new Exception("Sin completions la lista debe estar vacía");
foreach (var (position, category) in new (int?, string)[] {
    (1,"Main List"), (75,"Main List"), (76,"Extended"), (150,"Extended"), (151,"Legacy"), (1606,"Legacy"), (null,"Sin posición")
})
    if (new UruguayLevelListItemViewModel { PosicionAredl = position }.Categoria != category)
        throw new Exception($"Categoría incorrecta: {position}");
using var db = new UdlDbContext(new DbContextOptionsBuilder<UdlDbContext>().UseSqlServer("Server=localhost;Database=UDL;Integrated Security=True;TrustServerCertificate=True").Options);
var sql = UruguayLevelListQuery.Build(db.Nivel.AsNoTracking(), db.Record.AsNoTracking()).ToQueryString();
if (!sql.Contains("EXISTS") || !sql.Contains("COUNT(") || !sql.Contains("ORDER BY"))
    throw new Exception("La consulta no se tradujo a SQL");
Console.WriteLine("OK: solo niveles con 100%, exclusión de progreso/ausentes, orden y desempate, puntos de Nivel, victors, categorías límite y traducción SQL.");
Console.WriteLine(sql);
