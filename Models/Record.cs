using System;
using System.Collections.Generic;

namespace UDL.Models;

public partial class Record
{
    public int IdRecord { get; set; }

    public int IdJugador { get; set; }

    public int IdNivel { get; set; }

    public byte Porcentaje { get; set; }

    public string VideoUrl { get; set; } = null!;

    public string? RawFootageUrl { get; set; }

    public int? Fps { get; set; }

    public string? Dispositivo { get; set; }

    public DateTime FechaAprobacion { get; set; }

    public int IdSubmissionOrigen { get; set; }

    public virtual Jugador IdJugadorNavigation { get; set; } = null!;

    public virtual Nivel IdNivelNavigation { get; set; } = null!;

    public virtual Submission IdSubmissionOrigenNavigation { get; set; } = null!;
}
