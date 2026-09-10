using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UDL.Data;

namespace UDL.Services.Import;

/// <summary>Solo SELECT. Compatible con SQL anterior al ALTER y con el esquema nuevo.
/// No materializa Jugador vía EF, ya que AvatarUrl puede no existir todavía.</summary>
public sealed class UdlImportDatabaseReader(UdlDbContext db)
{
    public async Task<ImportDatabase> ReadAsync(CancellationToken ct = default)
    {
        var connection = db.Database.GetDbConnection();
        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await using var command = connection.CreateCommand();
            command.Transaction = db.Database.CurrentTransaction?.GetDbTransaction();
            command.CommandText = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Jugador') AND name = N'AvatarUrl';";
            var hasAvatar = Convert.ToInt32(await command.ExecuteScalarAsync(ct)) == 1;
            command.CommandText = hasAvatar
                ? "SELECT IdJugador, NombreGD, AvatarUrl FROM dbo.Jugador ORDER BY IdJugador;"
                : "SELECT IdJugador, NombreGD, CAST(NULL AS nvarchar(500)) AS AvatarUrl FROM dbo.Jugador ORDER BY IdJugador;";
            var players = new List<ExistingPlayer>();
            await using (var reader = await command.ExecuteReaderAsync(ct))
                while (await reader.ReadAsync(ct))
                    players.Add(new(reader.GetInt32(0), reader.GetString(1), reader.IsDBNull(2) ? null : reader.GetString(2)));
            command.CommandText = "SELECT IdNivel, AredlId, Nombre, PosicionAredl, TwoPlayer, GeometryDashId, Publisher FROM dbo.Nivel ORDER BY IdNivel;";
            var levels = new List<ExistingLevel>();
            await using (var reader = await command.ExecuteReaderAsync(ct))
                while (await reader.ReadAsync(ct))
                    levels.Add(new(reader.GetInt32(0), reader.GetGuid(1), reader.GetString(2),
                        reader.IsDBNull(3) ? null : reader.GetInt32(3), reader.GetBoolean(4),
                        reader.GetInt32(5), reader.IsDBNull(6) ? null : reader.GetString(6)));
            command.CommandText = "SELECT IdJugador, IdNivel, Porcentaje FROM dbo.Record ORDER BY IdJugador, IdNivel;";
            var records = new List<ExistingRecord>();
            await using (var reader = await command.ExecuteReaderAsync(ct))
                while (await reader.ReadAsync(ct))
                    records.Add(new(reader.GetInt32(0), reader.GetInt32(1), reader.GetByte(2)));
            return new(hasAvatar, players, levels, records);
        }
        finally { await db.Database.CloseConnectionAsync(); }
    }
}
