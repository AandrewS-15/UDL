using System;
using System.Collections.Generic;

namespace UDL.Models;

public partial class Submission
{
    public int IdSubmission { get; set; }

    public int IdJugador { get; set; }

    public int IdNivel { get; set; }

    public byte Porcentaje { get; set; }

    public string VideoUrl { get; set; } = null!;

    public string? RawFootageUrl { get; set; }

    public int? Fps { get; set; }

    public string? Dispositivo { get; set; }

    public string? Notas { get; set; }

    public string Estado { get; set; } = null!;

    public DateTime FechaEnvio { get; set; }

    public DateTime? FechaRevision { get; set; }

    public string? IdModerador { get; set; }

    public string? MotivoRechazo { get; set; }

    public virtual Jugador IdJugadorNavigation { get; set; } = null!;

    public virtual Nivel IdNivelNavigation { get; set; } = null!;

    public virtual ICollection<Record> Record { get; set; } = new List<Record>();
}
