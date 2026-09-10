namespace UDL.Services.Aredl;

public sealed record AredlSyncResult(int Nuevos, int Actualizados, int SinCambios,
    int Errores, int Historiales, int NivelesAredlAlmacenados, int LocalesFueraDeRespuesta,
    IReadOnlyList<string> Detalles);
