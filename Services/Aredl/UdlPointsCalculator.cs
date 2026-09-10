namespace UDL.Services.Aredl;

/// <summary>Fórmula de la planilla Uruguay Demon List; independiente de los puntos de AREDL.</summary>
public static class UdlPointsCalculator
{
    public static decimal Calculate(int position, int totalLevels)
    {
        if (totalLevels <= 0) throw new ArgumentOutOfRangeException(nameof(totalLevels));
        if (position <= 0 || position > totalLevels)
            throw new ArgumentOutOfRangeException(nameof(position));

        var points = position <= 75
            ? 4000 + 6000 * (Math.Exp(4.0 * (75 - position) / 74) - 1) / (Math.Exp(4) - 1)
            : position <= 150
                ? 1000 + 2500 * (Math.Exp(3.0 * (150 - position) / 74) - 1) / (Math.Exp(3) - 1)
                : 10 + 990 * (Math.Exp(7.0 * (totalLevels - position) / (totalLevels - 150)) - 1)
                    / (Math.Exp(7) - 1);

        // ROUND de Sheets/Excel: mitades alejándose de cero, no redondeo bancario.
        return (decimal)Math.Round(points, 1, MidpointRounding.AwayFromZero);
    }
}
