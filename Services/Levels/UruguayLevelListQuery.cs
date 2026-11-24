using Microsoft.EntityFrameworkCore;
using UDL.Data;
using UDL.Models;
using UDL.ViewModels;

namespace UDL.Services.Levels;

public sealed class UruguayLevelListQuery(UdlDbContext db)
{
    // Regla de inclusión solicitada: EXISTS Record del nivel con Porcentaje=100.
    // Record representa completions aprobadas, incluso históricos sin Submission.
    public static IQueryable<UruguayLevelListItemViewModel> Build(IQueryable<Nivel> levels, IQueryable<Record> records) =>
        levels.Where(n => records.Any(r => r.IdNivel == n.IdNivel && r.Porcentaje == 100))
            .OrderBy(n => n.PosicionAredl).ThenBy(n => n.Nombre)
            .Select(n => new UruguayLevelListItemViewModel
            {
                IdNivel = n.IdNivel, Nombre = n.Nombre, PosicionAredl = n.PosicionAredl, GeometryDashId = n.GeometryDashId,
                PuntosAredl = n.PuntosAredl,
                VictorCount = records.Count(r => r.IdNivel == n.IdNivel && r.Porcentaje == 100)
            });

    public async Task<IReadOnlyList<UruguayLevelListItemViewModel>> GetAsync(CancellationToken ct = default)
    {
        var result = await Build(db.Nivel.AsNoTracking(), db.Record.AsNoTracking()).ToListAsync(ct);
        for (var i = 0; i < result.Count; i++) result[i].PosicionUy = i + 1;
        return result;
    }
}
