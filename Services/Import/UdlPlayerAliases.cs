namespace UDL.Services.Import;

public static class UdlPlayerAliases
{
    // Decisión manual: no usar sugerencias tipográficas para decidir identidad.
    public static string Canonical(string victor) => ImportNames.Victor(victor) switch
    {
        "OKRUN" => "Okarun",
        _ => ImportNames.Clean(victor)
    };
}
