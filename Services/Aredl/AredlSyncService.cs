using Microsoft.EntityFrameworkCore;
using UDL.Data;
using UDL.Models;

namespace UDL.Services.Aredl;

public sealed class AredlSyncService(AredlService aredl, UdlDbContext db)
{
    public async Task<AredlSyncResult> SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        // HTTP y validación antes de abrir la transacción. No hay escrituras parciales.
        var response = await aredl.GetLevelsAsync(cancellationToken);
        var errors = new List<string>();
        var incoming = new Dictionary<Guid, Nivel>();
        // n pertenece al catálogo remoto completo, nunca al subconjunto guardado en SQL.
        var totalRankedLevels = response.Count(level => level.Position is > 0);
        if (response.Count == 0) errors.Add("AREDL devolvió un catálogo vacío; sincronización cancelada.");
        foreach (var dto in response)
        {
            try
            {
                if (!incoming.TryAdd(dto.Id, AredlLevelMapping.Map(dto, totalRankedLevels)))
                    errors.Add($"AredlId duplicado en respuesta: {dto.Id}.");
            }
            catch (ArgumentException ex) { errors.Add(ex.Message); }
        }
        if (errors.Count > 0) return new(0, 0, 0, errors.Count, 0, 0, 0, errors);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        // Mutex SQL compartido entre procesos; se libera con commit/rollback.
        await db.Database.ExecuteSqlRawAsync("""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource = N'UDL.AredlCatalogSync',
                @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000;
            IF @result < 0 THROW 51000, 'No se pudo obtener el bloqueo de sincronizacion AREDL.', 1;
            """, cancellationToken);
        var local = await db.Nivel.ToDictionaryAsync(n => n.AredlId, cancellationToken);
        var now = DateTime.UtcNow;
        var added = 0; var updated = 0; var unchanged = 0; var histories = 0;
        foreach (var source in incoming.Values)
        {
            if (!local.TryGetValue(source.AredlId, out var target))
            {
                source.UltimaSincronizacion = now;
                db.Nivel.Add(source);
                added++;
                continue;
            }
            var previous = AredlLevelMapping.PreviousState(target, source, now);
            if (previous is not null)
            {
                // Fecha indica cuándo se reemplazó este estado anterior (UTC).
                // El estado vigente queda en Nivel. No hay historial para inserts/no-op.
                db.HistorialNivel.Add(previous);
                histories++;
            }
            if (AredlLevelMapping.Apply(target, source)) updated++; else unchanged++;
            target.UltimaSincronizacion = now;
        }
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(added, updated, unchanged, 0, histories, incoming.Count,
            local.Keys.Count(id => !incoming.ContainsKey(id)), Array.Empty<string>());
    }
}
