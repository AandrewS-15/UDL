using Microsoft.EntityFrameworkCore;
using UDL.Data;
using UDL.Services.Ranking;
using UDL.ViewModels;

namespace UDL.Services.Levels;

public sealed class LevelDetailsService(UdlDbContext db)
{
    public async Task<LevelDetailsViewModel?> GetAsync(int id, CancellationToken ct = default)
    {
        var level = await db.Nivel.AsNoTracking().Where(n => n.IdNivel == id)
            .Select(n => new LevelDetailsViewModel
            {
                IdNivel = n.IdNivel, Nombre = n.Nombre, PosicionAredl = n.PosicionAredl,
                PuntosAredl = n.PuntosAredl, Publisher = n.Publisher, GeometryDashId = n.GeometryDashId,
                TwoPlayer = n.TwoPlayer, Description = n.Descripcion, TagsRaw = n.Tags,
                Song = n.SongId, EDEL = n.EdelEnjoyment, GDDL = n.GddlTier, NLW = n.NlwTier
            }).SingleOrDefaultAsync(ct);
        if (level is null) return null;
        // Misma inclusión y orden que la lista, proyectando únicamente IDs.
        var orderedIds = await UruguayLevelListQuery.Build(db.Nivel.AsNoTracking(), db.Record.AsNoTracking())
            .Select(n => n.IdNivel).ToListAsync(ct);
        var index = orderedIds.IndexOf(id);
        level.PosicionUy = index >= 0 ? index + 1 : null;
        var victors = db.Jugador.AsNoTracking().Where(j => db.Record.Any(r =>
            r.IdJugador == j.IdJugador && r.IdNivel == id && r.Porcentaje == 100));
        level.Victors = await PlayerRankingQuery.Build(victors, db.Record.AsNoTracking())
            .OrderByDescending(p => p.TotalPuntos).ThenBy(p => p.NombreGD).ToListAsync(ct);
        return level;
    }
}
