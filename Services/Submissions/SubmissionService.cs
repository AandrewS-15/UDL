using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using UDL.Data;
using UDL.Models;
using UDL.ViewModels;

namespace UDL.Services.Submissions;

public sealed record SubmissionResult(int? Id, List<ValidationResult> Errors);

public sealed class SubmissionService(UdlDbContext db)
{
    public Task<int?> PlayerAsync(string userId, CancellationToken ct = default) => db.Users.AsNoTracking()
        .Where(u => u.Id == userId).Select(u => u.IdJugador).SingleOrDefaultAsync(ct);

    private Task<SubmissionLevelViewModel?> LevelAsync(int id, CancellationToken ct) => db.Nivel.AsNoTracking()
        .Where(n => n.IdNivel == id).Select(n => new SubmissionLevelViewModel(n.IdNivel, n.Nombre, n.PosicionAredl, n.RequiresRawFootage))
        .SingleOrDefaultAsync(ct);

    public async Task PrepareAsync(CreateSubmissionViewModel model, string? search, CancellationToken ct)
    {
        model.Level = await LevelAsync(model.IdNivel, ct);
        model.Search = (search ?? "").Trim();
        if (model.Search.Length > 100) model.Search = model.Search[..100];
        model.Results = model.Search.Length == 0 ? [] : await db.Nivel.AsNoTracking().Where(n => n.Nombre.Contains(model.Search))
            .OrderBy(n => n.Nombre).ThenBy(n => n.IdNivel).Take(20)
            .Select(n => new SubmissionLevelViewModel(n.IdNivel, n.Nombre, n.PosicionAredl, n.RequiresRawFootage)).ToListAsync(ct);
    }

    public async Task<SubmissionResult> CreateAsync(string userId, CreateSubmissionViewModel input, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // SQL Server application lock, liberado al finalizar la transacción. No requiere cambios de schema.
        // Serializa todos los envíos de un jugador antes de comprobar duplicados e insertar.
        var playerId = await PlayerAsync(userId, ct);
        if (playerId is null) return Error("Primero debés vincular tu cuenta con un jugador.");
        var resource = $"UDL:Submission:Player:{playerId}";
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @r int; EXEC @r = sys.sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=10000; IF @r < 0 THROW 51010, 'No se pudo obtener el bloqueo de envío.', 1;", ct);
        // Releer después de esperar el bloqueo por si la cuenta fue desvinculada.
        if (await PlayerAsync(userId, ct) != playerId) return Error("El vínculo cambió. Revisá tu cuenta antes de enviar.");
        var level = await LevelAsync(input.IdNivel, ct);
        var errors = SubmissionRules.Validate(input, level);
        if (errors.Count > 0) return new(null, errors);
        if (await db.Submission.AnyAsync(s => s.IdJugador == playerId && s.IdNivel == input.IdNivel
            && s.Porcentaje == 100 && s.Estado == "Pendiente", ct))
            return Error("Ya tenés una completion pendiente para este nivel.");
        var submission = new Submission
        {
            IdJugador = playerId.Value, IdNivel = input.IdNivel, Porcentaje = 100,
            VideoUrl = input.VideoUrl.Trim(), RawFootageUrl = Clean(input.RawFootageUrl), Fps = input.FPS,
            Dispositivo = Clean(input.Dispositivo), Notas = Clean(input.Notas), Estado = "Pendiente",
            FechaEnvio = DateTime.UtcNow, FechaRevision = null, IdModerador = null, MotivoRechazo = null
        };
        db.Submission.Add(submission);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return new(submission.IdSubmission, []);
    }

    public async Task<SubmissionHistoryViewModel?> HistoryAsync(string userId, int page, CancellationToken ct)
    {
        var playerId = await PlayerAsync(userId, ct);
        if (playerId is null) return null;
        page = Math.Clamp(page, 1, 1000000);
        var items = await db.Submission.AsNoTracking().Where(s => s.IdJugador == playerId)
            .OrderByDescending(s => s.FechaEnvio).ThenByDescending(s => s.IdSubmission)
            .Skip((page - 1) * 50).Take(51)
            .Select(s => new SubmissionHistoryItemViewModel(s.IdSubmission, s.IdNivel, s.IdNivelNavigation.Nombre,
                s.Porcentaje, s.Estado, s.FechaEnvio, s.VideoUrl, s.MotivoRechazo)).ToListAsync(ct);
        return new(items.Take(50).ToList(), page, items.Count > 50);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static SubmissionResult Error(string message) => new(null, [new ValidationResult(message)]);
}
