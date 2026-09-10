namespace UDL.Services.Import;

/// <summary>Aliases explícitos revisados. Solo se consultan si no hay coincidencia exacta.
/// Las claves y destinos respetan mayúsculas. No se generan a partir de candidatos.</summary>
public static class UdlLevelAliases
{
    public static string? Target(string normalizedName) => normalizedName switch
    {
        "Heartbeat" => "Heartbeat (KrmaL)",
        // Identidad histórica confirmada por el usuario; no corresponde a ErebuS (Platnuu).
        "Erebus" => "Erebus (BoldStep)",
        _ => null
    };
}
