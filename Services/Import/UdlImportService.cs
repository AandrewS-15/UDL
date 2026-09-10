using System.Data;
using Microsoft.EntityFrameworkCore;
using UDL.Data;
using UDL.Models;

namespace UDL.Services.Import;

public sealed class UdlImportService(UdlDbContext db)
{
    public async Task<ImportPlan> ApplyAsync(SheetInput sheet, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await db.Database.ExecuteSqlRawAsync("""
            DECLARE @result int;
            EXEC @result = sys.sp_getapplock @Resource=N'UDL.HistoricalImport',
                @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=15000;
            IF @result < 0 THROW 51000, 'No se obtuvo bloqueo de importacion.', 1;
            """, ct);
        // Volver a resolver dentro de la transacción: nunca aplicar un plan SQL obsoleto.
        var database = await new UdlImportDatabaseReader(db).ReadAsync(ct);
        var plan = new UdlImportPlanner().Build(sheet, database);
        if (!database.HasAvatarColumn || plan.Conflicts.Count != 0)
            throw new InvalidOperationException("Importación cancelada: esquema pendiente o conflictos sin resolver.");
        var players = await db.Jugador.ToDictionaryAsync(p => p.IdJugador, ct);
        var targets = new Dictionary<string, Jugador>();
        var now = DateTime.UtcNow;
        foreach (var source in plan.Players)
        {
            Jugador player;
            if (source.ExistingId is int id)
            {
                player = players[id];
                if (source.AvatarAction == "Podría actualizarse") player.AvatarUrl = source.AvatarUrl;
            }
            else
            {
                player = new Jugador { NombreGd = source.Username, AvatarUrl = source.AvatarUrl,
                    Activo = true, EsUruguayo = true, FechaAlta = now };
                db.Jugador.Add(player);
            }
            targets.Add(source.Key, player);
        }
        await db.SaveChangesAsync(ct);
        foreach (var source in plan.Records.Where(r => r.Status == "Nuevo"))
            db.Record.Add(new Record
            {
                IdJugador = targets[ImportNames.Username(source.CanonicalUsername!)].IdJugador,
                IdNivel = source.LevelId!.Value, Porcentaje = 100,
                VideoUrl = null, RawFootageUrl = null, IdSubmissionOrigen = null,
                // Momento de incorporación, no fecha histórica de la completion.
                FechaAprobacion = now
            });
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return plan;
    }
}
