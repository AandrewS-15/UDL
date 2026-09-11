using Microsoft.EntityFrameworkCore;
using UDL.Data;
using UDL.Models;
using UDL.ViewModels;

namespace UDL.Services.Ranking;

public sealed class PlayerRankingQuery(UdlDbContext db)
{
    // Record representa el record aprobado, incluidos históricos sin Submission.
    // Agregados correlacionados en un único SELECT; sin materializar records ni N+1.
    public static IQueryable<PlayerRankingViewModel> Build(IQueryable<Jugador> players, IQueryable<Record> records)
    {
        return players.Select(player => new PlayerRankingViewModel
                {
                    IdJugador = player.IdJugador, NombreGD = player.NombreGd, AvatarUrl = player.AvatarUrl,
                    TotalPuntos = records.Where(r => r.IdJugador == player.IdJugador && r.Porcentaje == 100)
                        .Sum(r => (decimal?)r.IdNivelNavigation.PuntosAredl) ?? 0m,
                    CantidadCompletions = records.Count(r => r.IdJugador == player.IdJugador && r.Porcentaje == 100)
                })
            .OrderByDescending(p => p.TotalPuntos).ThenByDescending(p => p.CantidadCompletions).ThenBy(p => p.NombreGD);
    }

    public async Task<IReadOnlyList<PlayerRankingViewModel>> GetAsync(CancellationToken ct = default)
    {
        var rows = await Build(db.Jugador.AsNoTracking(), db.Record.AsNoTracking()).ToListAsync(ct);
        for (var i = 0; i < rows.Count; i++) rows[i].Posicion = i + 1;
        return rows;
    }
}
