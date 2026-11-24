using Microsoft.EntityFrameworkCore;
using UDL.Data;
using UDL.Models;
using UDL.Services.Home;
using UDL.Services.Ranking;
using UDL.ViewModels;

void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
var date = new DateTime(2026, 9, 10, 12, 0, 0);
Record R(int id, int? submission, byte percent) => new() { IdRecord = id, IdSubmissionOrigen = submission,
    Porcentaje = percent, FechaAprobacion = date.AddMinutes(id),
    IdJugadorNavigation = new Jugador { NombreGd = "Test" }, IdNivelNavigation = new Nivel { Nombre = "Level" } };
var records = new[] { R(10,null,100), R(9,1,99), R(1,1,100), R(2,2,100), R(3,3,100), R(4,4,100) };
Check(HomeService.RecentRecords(records.AsQueryable()).Select(r => r.IdRecord).SequenceEqual(new[] {4,3,2}), "Filtro de históricos/progresos, orden y límite");
Check(!HomeService.RecentRecords(new[] { R(10,null,100) }.AsQueryable()).Any(), "Importados no son actividad reciente");
Check(new HomeViewModel().RecentLevelChanges.Count == 0, "Sin eventos UDL el bloque queda vacío");
Console.WriteLine("OK: históricos y progresos excluidos; orden/límite de actividad; cambios UY vacíos.");
if (!args.Contains("--database")) return;
var commands = new List<string>();
using var db = new UdlDbContext(new DbContextOptionsBuilder<UdlDbContext>().UseSqlServer("Server=localhost;Database=UDL;Integrated Security=True;TrustServerCertificate=True").LogTo(commands.Add, new[] { Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuted }).Options);
var home = await new HomeService(db).GetAsync();
Check(!commands.Any(c => c.Contains("HistorialNivel")), "Home no debe consultar HistorialNivel");
Check(commands.Count == 2, "Home solo consulta ranking y records recientes");
Check(home.RecentLevelChanges.Count == 0, "No generar cambios UY desde records importados ni historial AREDL");
var ranking = await new PlayerRankingQuery(db).GetAsync();
Check(home.TopPlayers.Select(p => (p.IdJugador,p.TotalPuntos,p.Posicion)).SequenceEqual(ranking.Take(3).Select(p => (p.IdJugador,p.TotalPuntos,p.Posicion))), "Top3 no coincide ranking");
var expectedRecords = await db.Record.AsNoTracking().Where(r=>r.IdSubmissionOrigen!=null && r.Porcentaje==100).OrderByDescending(r=>r.FechaAprobacion).ThenByDescending(r=>r.IdRecord).Take(3).Select(r=>r.IdRecord).ToListAsync();
Check(home.RecentRecords.Select(r=>r.IdRecord).SequenceEqual(expectedRecords), "Records incorrectos");
foreach(var p in home.TopPlayers) Console.WriteLine($"TOP {p.Posicion}: {p.NombreGD} {p.TotalPuntos:0.0} /players/{p.IdJugador}");
Console.WriteLine($"Records recientes reales: {home.RecentRecords.Count}");
Console.WriteLine("Cambios UY: 0. Home no consulta HistorialNivel (2 consultas totales).");
Check(!db.ChangeTracker.HasChanges(), "Cambios inesperados");
Console.WriteLine("OK: Home contra SQL y ranking, solo lectura.");
