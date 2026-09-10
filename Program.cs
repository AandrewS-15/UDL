using Microsoft.EntityFrameworkCore;
using UDL.Data;
using Microsoft.Extensions.Options;
using UDL.Services.Aredl;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<AredlSyncService>();
builder.Services.AddOptions<AredlOptions>()
    .Bind(builder.Configuration.GetSection(AredlOptions.SectionName))
    .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps && string.IsNullOrEmpty(uri.Query)
        && string.IsNullOrEmpty(uri.Fragment), "Aredl:BaseUrl debe ser una URL HTTPS absoluta sin query ni fragmento.")
    .Validate(options => options.TimeoutSeconds is > 0 and <= 120,
        "Aredl:TimeoutSeconds debe estar entre 1 y 120.")
    .ValidateOnStart();
builder.Services.AddHttpClient<AredlService>((services, client) =>
{
    var options = services.GetRequiredService<IOptions<AredlOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});
builder.Services.AddDbContext<UdlDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("UDL")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:UDL.")));
var app = builder.Build();
// Ejecución manual explícita: no endpoint web ni trabajo programado.
if (args.Contains("--sync-aredl"))
{
    if (!app.Environment.IsDevelopment())
    {
        Console.Error.WriteLine("La sincronización manual solo está disponible en Development.");
        Environment.ExitCode = 1;
        return;
    }
    using var scope = app.Services.CreateScope();
    try
    {
        var result = await scope.ServiceProvider.GetRequiredService<AredlSyncService>()
            .SynchronizeAsync();
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(result));
        if (result.Errores > 0) Environment.ExitCode = 1;
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "La sincronización falló. La transacción no se confirmó.");
        Console.Error.WriteLine("Errores: 1. No se confirmó la sincronización; revisar el log.");
        Environment.ExitCode = 1;
    }
    return;
}
// Comprobación explícita de solo lectura. No se ejecuta durante el inicio normal.
if (args.Contains("--verify-database"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<UdlDbContext>();
    await db.Jugador.AsNoTracking().OrderBy(e => e.IdJugador).Take(1).ToListAsync();
    await db.Nivel.AsNoTracking().OrderBy(e => e.IdNivel).Take(1).ToListAsync();
    await db.Submission.AsNoTracking().OrderBy(e => e.IdSubmission).Take(1).ToListAsync();
    await db.Record.AsNoTracking().OrderBy(e => e.IdRecord).Take(1).ToListAsync();
    await db.HistorialNivel.AsNoTracking().OrderBy(e => e.IdHistorial).Take(1).ToListAsync();
    Console.WriteLine("Conexión y consultas EF de las cinco tablas verificadas (solo lectura).");
    return;
}
app.UseStaticFiles();
app.MapGet("/index.html", () => Results.Redirect("/"));
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
