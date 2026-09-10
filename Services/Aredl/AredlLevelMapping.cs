using System.Text.Json;
using UDL.Dtos.Aredl;
using UDL.Models;

namespace UDL.Services.Aredl;

public static class AredlLevelMapping
{
    public static Nivel Map(AredlLevelDto dto, int totalRankedLevels)
    {
        if (dto.Id == Guid.Empty || string.IsNullOrWhiteSpace(dto.Name) || dto.Name.Length > 200
            || string.IsNullOrWhiteSpace(dto.Status) || dto.Status.Length > 20
            || dto.Position is <= 0
            || dto.NlwTier?.Length > 20 || dto.Tags is null)
            throw new ArgumentException($"Nivel {dto.Id}: datos incompatibles con el esquema SQL.");
        return new Nivel
        {
            AredlId = dto.Id, GeometryDashId = dto.LevelId, Nombre = dto.Name,
            PosicionAredl = dto.Position,
            // Sin posición no participa del ranking ni recibe puntos UDL.
            PuntosAredl = dto.Position is int position
                ? UdlPointsCalculator.Calculate(position, totalRankedLevels) : 0m,
            EstadoAredl = dto.Status,
            RequiresRawFootage = dto.RequiresRawFootage, TwoPlayer = dto.TwoPlayer,
            Descripcion = dto.Description, Tags = JsonSerializer.Serialize(dto.Tags),
            SongId = dto.Song, EdelEnjoyment = SqlDecimal(dto.EdelEnjoyment),
            GddlTier = SqlDecimal(dto.GddlTier), NlwTier = dto.NlwTier
        };
    }

    private static decimal? SqlDecimal(double? value)
    {
        if (value is null) return null;
        if (!double.IsFinite(value.Value) || value.Value < -999.99 || value.Value > 999.99)
            throw new ArgumentException("Valor externo fuera del rango decimal(5,2).");
        return decimal.Round((decimal)value.Value, 2, MidpointRounding.AwayFromZero);
    }

    public static bool Apply(Nivel target, Nivel source)
    {
        var changed = target.GeometryDashId != source.GeometryDashId || target.Nombre != source.Nombre
            || HasHistoryChange(target, source) || target.RequiresRawFootage != source.RequiresRawFootage
            || target.TwoPlayer != source.TwoPlayer || target.Descripcion != source.Descripcion
            || target.Tags != source.Tags || target.SongId != source.SongId
            || target.EdelEnjoyment != source.EdelEnjoyment || target.GddlTier != source.GddlTier
            || target.NlwTier != source.NlwTier;
        if (!changed) return false;
        target.GeometryDashId = source.GeometryDashId; target.Nombre = source.Nombre;
        target.PosicionAredl = source.PosicionAredl; target.PuntosAredl = source.PuntosAredl;
        target.EstadoAredl = source.EstadoAredl; target.RequiresRawFootage = source.RequiresRawFootage;
        target.TwoPlayer = source.TwoPlayer; target.Descripcion = source.Descripcion;
        target.Tags = source.Tags; target.SongId = source.SongId;
        target.EdelEnjoyment = source.EdelEnjoyment; target.GddlTier = source.GddlTier;
        target.NlwTier = source.NlwTier;
        // Publisher y Verifier se conservan: /levels no proporciona sus nombres.
        return true;
    }

    public static bool HasHistoryChange(Nivel target, Nivel source) =>
        target.PosicionAredl != source.PosicionAredl || target.PuntosAredl != source.PuntosAredl
        || target.EstadoAredl != source.EstadoAredl;

    public static HistorialNivel? PreviousState(Nivel target, Nivel source, DateTime replacedAt) =>
        HasHistoryChange(target, source) ? new HistorialNivel
        {
            IdNivel = target.IdNivel, PosicionAredl = target.PosicionAredl,
            PuntosAredl = target.PuntosAredl, EstadoAredl = target.EstadoAredl, Fecha = replacedAt
        } : null;
}
