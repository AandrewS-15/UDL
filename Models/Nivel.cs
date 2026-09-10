using System;
using System.Collections.Generic;

namespace UDL.Models;

public partial class Nivel
{
    public int IdNivel { get; set; }

    public Guid AredlId { get; set; }

    public int GeometryDashId { get; set; }

    public string Nombre { get; set; } = null!;

    public int? PosicionAredl { get; set; }

    public decimal PuntosAredl { get; set; }

    public string EstadoAredl { get; set; } = null!;

    public bool RequiresRawFootage { get; set; }

    public bool TwoPlayer { get; set; }

    public string? Descripcion { get; set; }

    public string? Tags { get; set; }

    public string? Publisher { get; set; }

    public string? Verifier { get; set; }

    public int? SongId { get; set; }

    public decimal? EdelEnjoyment { get; set; }

    public decimal? GddlTier { get; set; }

    public string? NlwTier { get; set; }

    public DateTime UltimaSincronizacion { get; set; }

    public virtual ICollection<HistorialNivel> HistorialNivel { get; set; } = new List<HistorialNivel>();

    public virtual ICollection<Record> Record { get; set; } = new List<Record>();

    public virtual ICollection<Submission> Submission { get; set; } = new List<Submission>();
}
