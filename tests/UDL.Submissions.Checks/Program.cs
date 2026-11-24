using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UDL.Data;
using UDL.Models;
using UDL.Services.Accounts;
using UDL.Services.Submissions;
using UDL.ViewModels;

var checks = 0;
void Check(bool condition, string label) { if (!condition) throw new Exception(label); checks++; }
CreateSubmissionViewModel Input(int? percent = 100) => new() { IdNivel = 1, Porcentaje = percent, VideoUrl = "https://example.invalid/video" };
foreach (var rank in new int?[] { 1,43,75,76,108,150,151,253,null,0 })
foreach (var percent in new[] { 100,99,80,50,1,0,101 })
    Check((SubmissionRules.Validate(Input(percent), new(1,"Test",rank,false)).Count == 0) == (rank > 0 && percent == 100), $"Regla #{rank}/{percent}");
var ordinary = new SubmissionLevelViewModel(1,"Test",253,false);
Check(SubmissionRules.Validate(Input(null),ordinary).Count > 0,"Porcentaje obligatorio");
Check(SubmissionRules.Validate(Input(),null).Count > 0,"Nivel inexistente");
Check(SubmissionRules.Validate(Input(),ordinary with { RequiresRawFootage = true }).Count > 0,"Raw obligatorio");
var raw = Input(); raw.RawFootageUrl="https://example.invalid/raw";
Check(SubmissionRules.Validate(raw,ordinary with { RequiresRawFootage = true }).Count == 0,"Raw válido");
foreach (var url in new[] { "javascript:alert(1)","file:///test","data:text/html,test","no-url","https://user:pass@example.invalid" })
{ var input=Input(); input.VideoUrl=url; Check(SubmissionRules.Validate(input,ordinary).Count > 0,"URL rechazada"); }
var invalid = Input(); invalid.FPS=0; invalid.Notas="<script>alert(1)</script>"; invalid.Dispositivo=new string('x',51);
Check(SubmissionRules.Validate(invalid,ordinary).Count >= 3,"FPS, texto plano, límites");
var longInput=Input(); longInput.Notas=new string('x',501); longInput.VideoUrl="https://example.invalid/"+new string('a',500);
Check(SubmissionRules.Validate(longInput,ordinary).Count >= 2,"Longitudes SQL");
Console.WriteLine($"OK: {checks} comprobaciones de reglas y validaciones.");
if (!args.Contains("--http")) return;

const string connection="Server=localhost;Database=UDL;Integrated Security=True;TrustServerCertificate=True";
var services=new ServiceCollection(); services.AddLogging(); services.AddDataProtection();
services.AddDbContext<UdlDbContext>(o=>o.UseSqlServer(connection)); services.AddUdlIdentity(true);
await using var provider=services.BuildServiceProvider(); await using var scope=provider.CreateAsyncScope();
var db=scope.ServiceProvider.GetRequiredService<UdlDbContext>(); var users=scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
var marker="udlsubmissiontest_"+Guid.NewGuid().ToString("N");
var password=Convert.ToHexString(RandomNumberGenerator.GetBytes(24))+"a!";
var accounts=new List<ApplicationUser>();
var fixtureIds=new List<Guid>();
var recordBefore=await db.Record.AsNoTracking().OrderBy(r=>r.IdRecord).Select(r=>new {r.IdRecord,r.IdJugador,r.IdNivel,r.Porcentaje,r.VideoUrl,r.RawFootageUrl,r.IdSubmissionOrigen,r.FechaAprobacion}).ToListAsync();
var submissionsBefore=await db.Submission.CountAsync();
var rankingBefore=System.Text.Json.JsonSerializer.Serialize(await new UDL.Services.Ranking.PlayerRankingQuery(db).GetAsync());
HttpClient Client()=>new(new HttpClientHandler { CookieContainer=new(),AllowAutoRedirect=false }) { BaseAddress=new("http://localhost:5083") };
using var first=Client(); using var second=Client(); using var anon=Client();
async Task<string> Token(HttpClient client,string path)
{
    var html=await client.GetStringAsync(path);
    var tag=Regex.Match(html,"<input[^>]*name=\"__RequestVerificationToken\"[^>]*>").Value;
    var token=WebUtility.HtmlDecode(Regex.Match(tag,"value=\"([^\"]+)\"").Groups[1].Value);
    Check(token.Length>0,"Token antiforgery"); return token;
}
async Task<HttpResponseMessage> Post(HttpClient client,string target,string tokenPath,Dictionary<string,string> fields)
{ fields["__RequestVerificationToken"]=await Token(client,tokenPath); return await client.PostAsync(target,new FormUrlEncodedContent(fields)); }
Dictionary<string,string> Fields(Nivel level,int percentage)=>new() { ["IdNivel"]=level.IdNivel.ToString(),["Porcentaje"]=percentage.ToString(),["VideoUrl"]="https://example.invalid/"+marker,["RawFootageUrl"]=level.RequiresRawFootage?"https://example.invalid/raw":"",["Notas"]=marker,["IdJugador"]="999999",["Estado"]="Aprobado",["IdModerador"]="fake",["FechaRevision"]="2020-01-01" };
try
{
    Check((await anon.GetAsync("/submissions/new")).StatusCode==HttpStatusCode.Redirect,"Anónimo bloqueado new");
    Check((await anon.GetAsync("/submissions/mine")).StatusCode==HttpStatusCode.Redirect,"Anónimo bloqueado mine");
    for(var i=0;i<2;i++)
    {
        var account=new ApplicationUser { UserName=marker+i,Email=marker+i+"@example.invalid" };
        Check((await users.CreateAsync(account,password)).Succeeded,"Cuenta temporal"); accounts.Add(account);
        Check((await users.AddToRoleAsync(account,"Usuario")).Succeeded,"Rol Usuario");
        var client=i==0?first:second;
        Check((await Post(client,"/account/login","/account/login",new() { ["Login"]=account.UserName,["Password"]=password })).StatusCode==HttpStatusCode.Redirect,"Login");
    }
    Check((await first.GetAsync("/submissions/new")).StatusCode==HttpStatusCode.Forbidden,"Sin jugador GET");
    Check((await first.GetAsync("/submissions/mine")).StatusCode==HttpStatusCode.Forbidden,"Sin jugador mine");
    Check((await Post(first,"/submissions/new","/account",new())).StatusCode==HttpStatusCode.Forbidden,"Sin jugador POST");
    var free=await db.Jugador.Where(p=>!db.Users.Any(u=>u.IdJugador==p.IdJugador)).OrderBy(p=>p.IdJugador).Take(2).ToListAsync();
    Check(free.Count==2,"Dos jugadores libres para pruebas");
    for(var i=0;i<2;i++) { accounts[i].IdJugador=free[i].IdJugador; Check((await users.UpdateAsync(accounts[i])).Succeeded,"Vínculo temporal"); }
    Check((await first.GetAsync("/submissions/new")).StatusCode==HttpStatusCode.OK,"Jugador vinculado");
    Check((await first.PostAsync("/submissions/new",new FormUrlEncodedContent([]))).StatusCode==HttpStatusCode.BadRequest,"CSRF obligatorio");
    var main=await db.Nivel.AsNoTracking().FirstAsync(n=>n.PosicionAredl==43);
    var extended=await db.Nivel.AsNoTracking().FirstAsync(n=>n.PosicionAredl==108);
    var legacy=await db.Nivel.AsNoTracking().FirstAsync(n=>n.PosicionAredl>=151 && !n.RequiresRawFootage);
    // Estos estados no existen hoy en los datos reales: fixtures explícitos, nunca editar niveles históricos.
    var rawLevel=new Nivel { AredlId=Guid.NewGuid(),GeometryDashId=int.MaxValue,Nombre=marker+"_raw",PosicionAredl=43,RequiresRawFootage=true,EstadoAredl="Activo",UltimaSincronizacion=DateTime.UtcNow };
    var unavailable=new Nivel { AredlId=Guid.NewGuid(),GeometryDashId=int.MaxValue-1,Nombre=marker+"_unavailable",PosicionAredl=null,EstadoAredl="Activo",UltimaSincronizacion=DateTime.UtcNow };
    fixtureIds.Add(rawLevel.AredlId); fixtureIds.Add(unavailable.AredlId);
    db.Nivel.AddRange(rawLevel,unavailable); await db.SaveChangesAsync();
    var searchHtml=await first.GetStringAsync("/submissions/new?search="+Uri.EscapeDataString(main.Nombre));
    Check(searchHtml.Contains($"levelId={main.IdNivel}"),"Búsqueda SQL");
    var formHtml=await first.GetStringAsync("/submissions/new?levelId="+main.IdNivel);
    Check(formHtml.Contains("Completion (100%)") && !formHtml.Contains("name=\"Porcentaje\""),"Formulario sin porcentaje editable ni oculto");
    foreach(var level in new[] { main,extended,legacy })
    foreach(var percentage in new[] { 99,80,50,1 })
    {
        var response=await Post(first,"/submissions/new","/submissions/new",Fields(level,percentage));
        Check(response.StatusCode==HttpStatusCode.OK && WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()).Contains("Solo se aceptan completions al 100%."),"Porcentaje rechazado HTTP");
    }
    var noRaw=Fields(rawLevel,100); noRaw["RawFootageUrl"]="";
    Check(WebUtility.HtmlDecode(await (await Post(first,"/submissions/new","/submissions/new",noRaw)).Content.ReadAsStringAsync()).Contains("requiere Raw Footage"),"Raw exigido SQL");
    Check(WebUtility.HtmlDecode(await (await Post(first,"/submissions/new","/submissions/new",Fields(unavailable,100))).Content.ReadAsStringAsync()).Contains("no está disponible"),"Posición NULL bloqueada SQL");
    var missing=Fields(main,100); missing["IdNivel"]=int.MaxValue.ToString();
    Check(WebUtility.HtmlDecode(await (await Post(first,"/submissions/new","/submissions/new",missing)).Content.ReadAsStringAsync()).Contains("no existe"),"Nivel inexistente HTTP");
    foreach(var (level,percentage) in new[] { (main,100),(extended,100),(legacy,100) })
        Check((await Post(first,"/submissions/new","/submissions/new",Fields(level,percentage))).StatusCode==HttpStatusCode.Redirect,"Envío válido HTTP");
    var duplicate=await Post(first,"/submissions/new","/submissions/new",Fields(main,100));
    Check(WebUtility.HtmlDecode(await duplicate.Content.ReadAsStringAsync()).Contains("completion pendiente"),"Duplicado exacto");
    var token=await Token(first,"/submissions/new");
    var concurrent=Fields(rawLevel,100); concurrent.Remove("Porcentaje"); concurrent["__RequestVerificationToken"]=token;
    var race=await Task.WhenAll(first.PostAsync("/submissions/new",new FormUrlEncodedContent(concurrent)), first.PostAsync("/submissions/new",new FormUrlEncodedContent(concurrent)));
    Check(race.Count(r=>r.StatusCode==HttpStatusCode.Redirect)==1,"Un ganador concurrente");
    Check(await db.Submission.CountAsync(s=>s.Notas==marker && s.IdNivel==rawLevel.IdNivel && s.Porcentaje==100)==1,"Sin duplicado concurrente en SQL");
    var rows=await db.Submission.AsNoTracking().Where(s=>s.Notas==marker).ToListAsync();
    Check(rows.Count==4 && rows.All(s=>s.Porcentaje==100 && s.IdJugador==free[0].IdJugador && s.Estado=="Pendiente" && s.FechaRevision==null && s.IdModerador==null && s.MotivoRechazo==null),"Campos sensibles decididos en servidor");
    Check(rows.Where(s=>s.IdNivel==legacy.IdNivel).All(s=>s.RawFootageUrl==null),"Raw opcional NULL");
    Check(rows.All(s=>s.FechaEnvio>DateTime.UtcNow.AddMinutes(-5)),"Fecha del servidor UTC");
    var own=await first.GetStringAsync("/submissions/mine?IdJugador="+free[1].IdJugador);
    var other=await second.GetStringAsync("/submissions/mine?IdJugador="+free[0].IdJugador);
    foreach(var row in rows) { Check(own.Contains($"data-submission-id=\"{row.IdSubmission}\""),"Historial propio"); Check(!other.Contains($"data-submission-id=\"{row.IdSubmission}\""),"Historial ajeno aislado"); }
    var after=await db.Record.AsNoTracking().OrderBy(r=>r.IdRecord).Select(r=>new {r.IdRecord,r.IdJugador,r.IdNivel,r.Porcentaje,r.VideoUrl,r.RawFootageUrl,r.IdSubmissionOrigen,r.FechaAprobacion}).ToListAsync();
    Check(recordBefore.SequenceEqual(after),"Record intacto");
    Check(rankingBefore==System.Text.Json.JsonSerializer.Serialize(await new UDL.Services.Ranking.PlayerRankingQuery(db).GetAsync()),"Ranking intacto tras enviar");
    Console.WriteLine($"OK: {checks} comprobaciones, incluyendo HTTP/SQL, antiforgery, overposting, aislamiento, duplicados concurrentes y Record intacto.");
}
finally
{
    // Solo filas identificadas por el marcador aleatorio exclusivo de esta ejecución.
    await db.Submission.Where(s=>s.Notas==marker && s.VideoUrl=="https://example.invalid/"+marker).ExecuteDeleteAsync();
    await db.Nivel.Where(n=>fixtureIds.Contains(n.AredlId) && n.Nombre.StartsWith(marker)).ExecuteDeleteAsync();
    foreach(var account in accounts) { db.ChangeTracker.Clear(); var current=await users.FindByIdAsync(account.Id); if(current is not null) Check((await users.DeleteAsync(current)).Succeeded,"Limpieza de cuenta temporal"); }
    Check(await db.Submission.CountAsync()==submissionsBefore,"Recuento Submission restaurado");
    Console.WriteLine("Limpieza terminada: sin submissions ni cuentas de prueba permanentes; jugadores y records sin modificar.");
}

