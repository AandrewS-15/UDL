using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UDL.Data;
using UDL.Models;
using UDL.Services.Aredl;
using UDL.Services.Ranking;

// Auditoría de solo lectura. No ejecuta ni reemplaza la sincronización existente.
using var db = new UdlDbContext(new DbContextOptionsBuilder<UdlDbContext>().UseSqlServer("Server=localhost;Database=UDL;Integrated Security=True;TrustServerCertificate=True").Options);
var levels = await db.Nivel.AsNoTracking().OrderBy(n=>n.IdNivel).ToListAsync();
var history = await db.HistorialNivel.AsNoTracking().OrderBy(h=>h.IdHistorial).ToListAsync();
var hashes = new Dictionary<string,string>();
await db.Database.OpenConnectionAsync();
foreach(var (table,key) in new[] { ("Jugador","IdJugador"),("Record","IdRecord"),("Submission","IdSubmission"),("AspNetUsers","Id"),("AspNetRoles","Id"),("AspNetUserRoles","UserId,RoleId"),("AspNetUserClaims","Id"),("AspNetRoleClaims","Id"),("AspNetUserLogins","LoginProvider,ProviderKey"),("AspNetUserTokens","UserId,LoginProvider,Name") })
{
    using var command=db.Database.GetDbConnection().CreateCommand();
    command.CommandText=$"SELECT * FROM dbo.[{table}] ORDER BY {key} FOR JSON PATH, INCLUDE_NULL_VALUES";
    using var reader=await command.ExecuteReaderAsync(); var content=new StringBuilder();
    while(await reader.ReadAsync()) content.Append(reader.GetString(0));
    hashes[table]=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content.ToString())));
}
var ranking=await new PlayerRankingQuery(db).GetAsync();
var current=new Snapshot(levels,history,hashes,JsonSerializer.Serialize(ranking));
var options=new JsonSerializerOptions { WriteIndented=true };
await File.WriteAllTextAsync(args[0],JsonSerializer.Serialize(current,options));
Console.WriteLine($"Snapshot: {levels.Count} niveles, {history.Count} historiales. Tablas protegidas: {hashes.Count}.");
if(args.Length<2) return;
var before=JsonSerializer.Deserialize<Snapshot>(await File.ReadAllTextAsync(args[1]))!;
var old=before.Levels.ToDictionary(n=>n.IdNivel);
var changed=levels.Where(n=>old.ContainsKey(n.IdNivel)&&AredlLevelMapping.HasHistoryChange(old[n.IdNivel],n)).ToList();
var addedHist=history.Where(h=>h.IdHistorial>(before.History.LastOrDefault()?.IdHistorial??0)).ToList();
bool historiesValid=addedHist.Count==changed.Count && changed.All(n=>addedHist.Count(h=>h.IdNivel==n.IdNivel && h.PosicionAredl==old[n.IdNivel].PosicionAredl && h.PuntosAredl==old[n.IdNivel].PuntosAredl && h.EstadoAredl==old[n.IdNivel].EstadoAredl)==1);
bool protectedValid=hashes.All(kv=>before.Hashes[kv.Key]==kv.Value);
int nRanked=levels.Count(n=>n.PosicionAredl>0);
int formulaErrors=levels.Count(n=>n.PuntosAredl!=(n.PosicionAredl is int pos?UdlPointsCalculator.Calculate(pos,nRanked):0m));
var stats=new { Posicion=changed.Count(n=>n.PosicionAredl!=old[n.IdNivel].PosicionAredl), Puntos=changed.Count(n=>n.PuntosAredl!=old[n.IdNivel].PuntosAredl), Estado=changed.Count(n=>n.EstadoAredl!=old[n.IdNivel].EstadoAredl), Nuevos=levels.Count(n=>!old.ContainsKey(n.IdNivel)), Historiales=addedHist.Count, HistorialCorrecto=historiesValid, TablasProtegidasIntactas=protectedValid, ErroresFormula=formulaErrors, TotalRankeados=nRanked };
Console.WriteLine(JsonSerializer.Serialize(stats,options));
var examples=changed.Where(n=>new[]{"Sakupen Circles","Tartarus","Bloodlust","Acu","Niwa"}.Contains(n.Nombre)).Select(n=>new { n.IdNivel,n.Nombre,Anterior=old[n.IdNivel].PosicionAredl,Nueva=n.PosicionAredl,PuntosAntes=old[n.IdNivel].PuntosAredl,PuntosNuevos=n.PuntosAredl }).ToList();
Console.WriteLine(JsonSerializer.Serialize(examples,options));
Console.WriteLine("TOP 10: "+JsonSerializer.Serialize(ranking.Take(10),options));
if(!historiesValid||!protectedValid||formulaErrors!=0) throw new Exception("Falló validación de auditoría");
record Snapshot(List<Nivel> Levels,List<HistorialNivel> History,Dictionary<string,string> Hashes,string Ranking);
