using Microsoft.EntityFrameworkCore;
using UDL.Data;
using UDL.Models;
using UDL.Services.Ranking;
using UDL.ViewModels;

namespace UDL.Services.Home;

public sealed class HomeService(UdlDbContext db)
{
    public static IQueryable<HomeRecordViewModel> RecentRecords(IQueryable<Record> records) => records
        .Where(r => r.IdSubmissionOrigen != null && r.Porcentaje == 100)
        .OrderByDescending(r => r.FechaAprobacion).ThenByDescending(r => r.IdRecord).Take(3)
        .Select(r => new HomeRecordViewModel
        {
            IdRecord = r.IdRecord, IdJugador = r.IdJugador, IdNivel = r.IdNivel,
            NombreGD = r.IdJugadorNavigation.NombreGd, NombreNivel = r.IdNivelNavigation.Nombre,
            FechaAprobacion = r.FechaAprobacion
        });

    public async Task<HomeViewModel> GetAsync(CancellationToken ct = default)
    {
        var players = await PlayerRankingQuery.Build(db.Jugador.AsNoTracking(), db.Record.AsNoTracking()).Take(3).ToListAsync(ct);
        for (var i = 0; i < players.Count; i++) players[i].Posicion = i + 1;
        var records = await RecentRecords(db.Record.AsNoTracking()).ToListAsync(ct);
        // Reservado para eventos UDL reales. HistorialNivel pertenece exclusivamente a AREDL.
        return new HomeViewModel { TopPlayers = players, RecentRecords = records, RecentLevelChanges = [] };
    }
}
