using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace UDL.Services.Aredl;

public sealed class AredlSyncBackgroundService(IServiceScopeFactory scopes, IOptions<AredlOptions> options,
    TimeProvider time, ILogger<AredlSyncBackgroundService> logger) : BackgroundService
{
    public static bool ShouldRegister(IEnumerable<string> args, string environment) =>
        !string.Equals(environment, "Testing", StringComparison.OrdinalIgnoreCase)
        && !args.Any(a => a is "--sync-aredl" or "--import-udl-sheet" or "--verify-database" or "--grant-role");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.SyncEnabled)
        {
            logger.LogInformation("Sincronización automática AREDL deshabilitada.");
            return;
        }
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(settings.SyncInitialDelaySeconds), time, stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                var watch = Stopwatch.StartNew();
                try
                {
                    logger.LogDebug("Inicio sincronización automática AREDL.");
                    await using var scope = scopes.CreateAsyncScope();
                    var result = await scope.ServiceProvider.GetRequiredService<IAredlSyncService>().SynchronizeAsync(stoppingToken);
                    logger.Log(result.Errores > 0 ? LogLevel.Warning : LogLevel.Information,
                        "AREDL automático: Nuevos {Nuevos}, Actualizados {Actualizados}, Sin cambios {SinCambios}, Historiales {Historiales}, Errores {Errores}. Duración {DuracionMs} ms.",
                        result.Nuevos, result.Actualizados, result.SinCambios, result.Historiales, result.Errores, watch.ElapsedMilliseconds);
                    if (result.Errores > 0) logger.LogWarning("Validación AREDL: {Detalles}", string.Join("; ", result.Detalles));
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { return; }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Falló sincronización automática AREDL después de {DuracionMs} ms. Se conservan los últimos datos válidos; se reintentará en el próximo ciclo.", watch.ElapsedMilliseconds);
                }
                // Esperar después de terminar: nunca solapar ciclos ni recuperar ticks acumulados.
                await Task.Delay(TimeSpan.FromMinutes(settings.SyncIntervalMinutes), time, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }
}
