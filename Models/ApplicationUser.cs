using Microsoft.AspNetCore.Identity;

namespace UDL.Models;

public sealed class ApplicationUser : IdentityUser
{
    public int? IdJugador { get; set; }
    public Jugador? Jugador { get; set; }
    // La asociación libre no acredita identidad ni concede permisos sobre records.
    public bool VinculoJugadorVerificado { get; set; }
}
