using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UDL.Data;
using UDL.Models;

namespace UDL.Services.Accounts;

public sealed class PlayerAccountLinkService(UdlDbContext db, UserManager<ApplicationUser> users)
{
    public async Task<IdentityResult> LinkAsync(ApplicationUser user, int playerId, CancellationToken ct = default)
    {
        if (user.IdJugador.HasValue) return Error("Tu cuenta ya tiene un jugador vinculado.");
        if (!await db.Jugador.AnyAsync(p => p.IdJugador == playerId, ct)) return Error("Jugador inexistente.");
        if (await db.Users.AnyAsync(u => u.IdJugador == playerId, ct)) return Error("Ese jugador ya está vinculado a otra cuenta.");
        user.IdJugador = playerId;
        user.VinculoJugadorVerificado = false;
        try
        {
            var result = await users.UpdateAsync(user);
            if (!result.Succeeded) await db.Entry(user).ReloadAsync(ct);
            return result;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // El índice único resuelve también dos solicitudes concurrentes.
            await db.Entry(user).ReloadAsync(ct);
            return Error("Ese jugador ya está vinculado a otra cuenta.");
        }
    }

    public async Task<IdentityResult> UnlinkAsync(ApplicationUser user)
    {
        if (user.VinculoJugadorVerificado) return Error("Un vínculo verificado requiere revisión para desvincularse.");
        user.IdJugador = null;
        user.VinculoJugadorVerificado = false;
        return await users.UpdateAsync(user);
    }

    private static IdentityResult Error(string description) => IdentityResult.Failed(new IdentityError { Description = description });
}
