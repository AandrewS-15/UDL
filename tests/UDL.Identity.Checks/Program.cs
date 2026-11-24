using System.Net;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UDL.Data;
using UDL.Models;
using UDL.Services.Accounts;

const string connection = "Server=localhost;Database=UDL;Integrated Security=True;TrustServerCertificate=True";
void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
using var modelDb = new UdlDbContext(new DbContextOptionsBuilder<UdlDbContext>().UseSqlServer(connection).Options);
var userModel = modelDb.Model.FindEntityType(typeof(ApplicationUser))!;
Check(userModel.GetIndexes().Any(i => i.IsUnique && i.Properties.SingleOrDefault()?.Name == "IdJugador" && i.GetFilter() == "[IdJugador] IS NOT NULL"), "Vínculo único filtrado");
Check(userModel.FindProperty("IdJugador")!.IsNullable, "Vínculo opcional");
Check(userModel.GetForeignKeys().Any(f => f.PrincipalEntityType.ClrType == typeof(Jugador) && f.DeleteBehavior == DeleteBehavior.NoAction), "FK no destructiva");
Check(modelDb.Model.FindEntityType(typeof(Record))!.GetKeys().Any(k => k.Properties.Select(p => p.Name).SequenceEqual(new[] { "IdJugador", "IdNivel" })), "Unique Record intacto");
Console.WriteLine("OK: modelo Identity, vínculo opcional/único, FK restrictiva, UNIQUE Record conservado.");
if (args.Contains("--write-schema"))
{
    var blocks = Regex.Split(modelDb.Database.GenerateCreateScript(), @"(?m)^GO\s*$")
        .Select(b => b.Trim()).Where(b => Regex.IsMatch(b, @"^CREATE (?:TABLE \[AspNet|(?:UNIQUE )?INDEX .* ON \[AspNet)", RegexOptions.Singleline)).ToArray();
    Check(blocks.Count(b => b.StartsWith("CREATE TABLE")) == 7, "Deben generarse exactamente siete tablas Identity");
    Check(blocks.All(b => !b.Contains("DROP ") && !b.Contains("ALTER ")), "Script exclusivamente aditivo");
    var script = """
        -- Solo tablas ASP.NET Core Identity. No modifica ninguna tabla UDL existente.
        -- Generado desde el modelo EF y revisado antes de aplicar. Aborta si Identity ya existe.
        SET ANSI_NULLS ON;
        SET QUOTED_IDENTIFIER ON;
        SET ANSI_PADDING ON;
        SET ANSI_WARNINGS ON;
        SET CONCAT_NULL_YIELDS_NULL ON;
        SET ARITHABORT ON;
        SET NUMERIC_ROUNDABORT OFF;
        SET XACT_ABORT ON;
        BEGIN TRY
        BEGIN TRANSACTION;
        IF EXISTS (SELECT 1 FROM sys.tables WHERE name LIKE 'AspNet%')
            THROW 51000, 'Ya existen tablas Identity; inspeccionar schema antes de continuar.', 1;
        IF OBJECT_ID(N'dbo.Jugador', N'U') IS NULL
            THROW 51001, 'Falta dbo.Jugador; no ejecutar contra otra base.', 1;
        """ + "\n" + string.Join("\n\n", blocks) + "\nCOMMIT;\nEND TRY\nBEGIN CATCH\nIF @@TRANCOUNT > 0 ROLLBACK;\nTHROW;\nEND CATCH;\n";
    await File.WriteAllTextAsync("sql/add-identity.sql", script);
    Console.WriteLine("SQL aditivo generado: sql/add-identity.sql");
    return;
}
if (!args.Contains("--http")) return;
var services = new ServiceCollection();
services.AddLogging();
services.AddDataProtection();
services.AddDbContext<UdlDbContext>(o => o.UseSqlServer(connection));
services.AddUdlIdentity(development: true);
await using var provider = services.BuildServiceProvider();
await IdentitySetup.SeedRolesAsync(provider);
await IdentitySetup.SeedRolesAsync(provider);
await using var scope = provider.CreateAsyncScope();
var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
var db = scope.ServiceProvider.GetRequiredService<UdlDbContext>();
Check(await db.Roles.CountAsync(r => IdentitySetup.Roles.Contains(r.Name!)) == 3, "Roles duplicados");
var stamp = Guid.NewGuid().ToString("N")[..12];
var names = new[] { "udltest_" + stamp + "a", "udltest_" + stamp + "b" };
var password = Convert.ToHexString(RandomNumberGenerator.GetBytes(24)) + "a!";
HttpClient Client() => new(new HttpClientHandler { CookieContainer = new CookieContainer(), AllowAutoRedirect = false }) { BaseAddress = new Uri("http://localhost:5083") };
using var first = Client(); using var second = Client(); using var anonymous = Client();
async Task<string> Token(HttpClient client, string path)
{
    var html = await client.GetStringAsync(path);
    var tag = Regex.Match(html, "<input[^>]*name=\"__RequestVerificationToken\"[^>]*>").Value;
    var value = Regex.Match(tag, "value=\"([^\"]+)\"").Groups[1].Value;
    Check(value.Length > 0, "Falta token antiforgery");
    return WebUtility.HtmlDecode(value);
}
async Task<HttpResponseMessage> Post(HttpClient client, string target, string tokenPath, Dictionary<string,string> fields)
{
    fields["__RequestVerificationToken"] = await Token(client, tokenPath);
    return await client.PostAsync(target, new FormUrlEncodedContent(fields));
}
try
{
    var protectedResponse = await anonymous.GetAsync("/account");
    Check(protectedResponse.StatusCode == HttpStatusCode.Redirect && protectedResponse.Headers.Location!.OriginalString.Contains("/account/login"), "Cuenta protegida");
    Check((await anonymous.PostAsync("/account/register", new FormUrlEncodedContent([]))).StatusCode == HttpStatusCode.BadRequest, "Antiforgery obligatorio");
    for (var i = 0; i < 2; i++)
    {
        var client = i == 0 ? first : second;
        var response = await Post(client, "/account/register", "/account/register", new() { ["UserName"]=names[i], ["Email"]=names[i]+"@example.invalid", ["Password"]=password, ["ConfirmPassword"]=password, ["Role"]="Administrador", ["IdJugador"]="1", ["VinculoJugadorVerificado"]="true" });
        Check(response.StatusCode == HttpStatusCode.Redirect, "Registro no completado");
        var user = await users.FindByNameAsync(names[i]) ?? throw new Exception("Usuario no creado");
        Check((await users.GetRolesAsync(user)).SequenceEqual(new[] { IdentitySetup.Usuario }), "Registro no debe escalar rol");
        Check(user.IdJugador is null && !user.VinculoJugadorVerificado, "Registro no debe aceptar campos de vinculación");
        Check(user.PasswordHash is not null && user.PasswordHash != password && await users.CheckPasswordAsync(user,password), "Hash Identity");
        Check(response.Headers.TryGetValues("Set-Cookie", out var cookies) && cookies.Any(c => c.Contains("UDL.Auth") && c.Contains("httponly", StringComparison.OrdinalIgnoreCase)), "Cookie HttpOnly");
        Check((await client.GetAsync("/account")).StatusCode == HttpStatusCode.OK, "Sesión tras registro");
    }
    Check((await first.GetAsync("/account/logout")).StatusCode == HttpStatusCode.MethodNotAllowed, "Logout GET prohibido");
    Check((await first.PostAsync("/account/logout",new FormUrlEncodedContent([]))).StatusCode == HttpStatusCode.BadRequest, "Logout antiforgery");
    Check((await Post(first,"/account/logout","/account",new())).StatusCode == HttpStatusCode.Redirect, "Logout POST");
    Check((await first.GetAsync("/account")).StatusCode == HttpStatusCode.Redirect, "Logout invalida sesión");
    var badLogin = await Post(first,"/account/login","/account/login",new() { ["Login"]=names[0],["Password"]=password+"incorrect" });
    Check(badLogin.StatusCode == HttpStatusCode.OK && (await badLogin.Content.ReadAsStringAsync()).Contains("No se pudo iniciar"), "Password incorrecta rechazada");
    Check((await first.GetAsync("/account")).StatusCode == HttpStatusCode.Redirect, "Login inválido no crea sesión");
    var goodLogin = await Post(first,"/account/login","/account/login",new() { ["Login"]=names[0]+"@example.invalid",["Password"]=password,["ReturnUrl"]="https://example.invalid/attack",["RememberMe"]="true" });
    Check(goodLogin.StatusCode == HttpStatusCode.Redirect && goodLogin.Headers.Location!.OriginalString == "/account", "Login/email y rechazo de open redirect");
    var free = await db.Jugador.Where(p=>!db.Users.Any(u=>u.IdJugador==p.IdJugador)).OrderBy(p=>p.IdJugador).Select(p=>new { p.IdJugador,p.NombreGd }).FirstAsync();
    Check((await Post(first,"/account/link","/account",new() { ["idJugador"]=free.IdJugador.ToString(), ["VinculoJugadorVerificado"]="true" })).StatusCode == HttpStatusCode.Redirect, "Vínculo libre");
    var linked = await db.Users.AsNoTracking().SingleAsync(u=>u.UserName==names[0]);
    Check(linked.IdJugador == free.IdJugador && !linked.VinculoJugadorVerificado, "Vínculo siempre provisional");
    Check((await Post(second,"/account/link","/account",new() { ["idJugador"]=free.IdJugador.ToString() })).StatusCode == HttpStatusCode.Conflict, "Jugador ya vinculado debe rechazarse");
    var search = await second.GetStringAsync("/account?search="+Uri.EscapeDataString(free.NombreGd));
    Check(!search.Contains($"name=\"idJugador\" value=\"{free.IdJugador}\""), "Búsqueda no ofrece ocupados");
    Check((await Post(first,"/account/unlink","/account",new())).StatusCode == HttpStatusCode.Redirect, "Desvinculación provisional");
    var firstToken = await Token(first, "/account");
    var secondToken = await Token(second, "/account");
    var race = await Task.WhenAll(
        first.PostAsync("/account/link", new FormUrlEncodedContent(new Dictionary<string,string> { ["idJugador"]=free.IdJugador.ToString(),["__RequestVerificationToken"]=firstToken })),
        second.PostAsync("/account/link", new FormUrlEncodedContent(new Dictionary<string,string> { ["idJugador"]=free.IdJugador.ToString(),["__RequestVerificationToken"]=secondToken })));
    Check(race.Count(r => r.StatusCode == HttpStatusCode.Redirect) == 1 && race.Count(r => r.StatusCode == HttpStatusCode.Conflict) == 1, "Dos vínculos concurrentes deben tener un único ganador");
    Check(await db.Users.CountAsync(u => u.IdJugador == free.IdJugador) == 1, "Índice único ante concurrencia");
    Check((await Post(first,"/account/logout","/account",new())).StatusCode == HttpStatusCode.Redirect, "Segundo logout POST");
    var usernameLogin = await Post(first,"/account/login","/account/login",new() { ["Login"]=names[0],["Password"]=password });
    Check(usernameLogin.StatusCode == HttpStatusCode.Redirect, "Login por username");
    Check((await Post(first,"/account/logout","/account",new())).StatusCode == HttpStatusCode.Redirect, "Logout antes del test de bloqueo");
    for (var attempt = 0; attempt < 5; attempt++)
        await Post(first,"/account/login","/account/login",new() { ["Login"]=names[0],["Password"]=password+"wrong" });
    var locked = await Post(first,"/account/login","/account/login",new() { ["Login"]=names[0],["Password"]=password });
    Check(locked.StatusCode == HttpStatusCode.OK && (await first.GetAsync("/account")).StatusCode == HttpStatusCode.Redirect, "Bloqueo temporal tras intentos fallidos");
    Console.WriteLine("OK HTTP: registro x2, rol Usuario (sin escalada), hash Identity, cookies, antiforgery, login email/username, password incorrecta/bloqueo, ruta protegida, retorno local, vínculo provisional, conflicto/concurrencia y desvinculación, logout POST; roles idempotentes.");
}
finally
{
    // Solo las cuentas sintéticas creadas por esta ejecución, nunca jugadores o records.
    foreach (var name in names)
    {
        db.ChangeTracker.Clear();
        var user = await users.FindByNameAsync(name);
        if (user is not null) Check((await users.DeleteAsync(user)).Succeeded, "No se pudo limpiar cuenta de test");
    }
    Console.WriteLine("Cuentas temporales de esta prueba eliminadas; sin credenciales persistidas en archivos.");
}
