using Microsoft.EntityFrameworkCore;
using UDL.Data;
using UDL.ViewModels;

namespace UDL.Services.Ranking;

public sealed class PlayerProfileService(UdlDbContext db, PlayerRankingQuery ranking)
{
    public async Task<PlayerProfileViewModel?> GetAsync(int id, CancellationToken ct = default)
    {
        var player = (await ranking.GetAsync(ct)).FirstOrDefault(p => p.IdJugador == id);
        if (player is null) return null;
        var completions = await db.Record.AsNoTracking()
            .Where(r => r.IdJugador == id && r.Porcentaje == 100)
            .OrderBy(r => r.IdNivelNavigation.PosicionAredl).ThenBy(r => r.IdNivelNavigation.Nombre)
            .Select(r => new UruguayLevelListItemViewModel
            {
                IdNivel = r.IdNivel, Nombre = r.IdNivelNavigation.Nombre,
                PosicionAredl = r.IdNivelNavigation.PosicionAredl,
                PuntosAredl = r.IdNivelNavigation.PuntosAredl, GeometryDashId = r.IdNivelNavigation.GeometryDashId
            }).ToListAsync(ct);
        return new PlayerProfileViewModel
        {
            IdJugador = id, NombreGD = player.NombreGD, AvatarUrl = player.AvatarUrl,
            PosicionUy = player.Posicion, TotalPuntos = player.TotalPuntos,
            CantidadCompletions = player.CantidadCompletions, Completions = completions
        };
    }
}
