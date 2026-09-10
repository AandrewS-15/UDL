using System;
using System.Collections.Generic;

namespace UDL.Models;

public partial class HistorialNivel
{
    public int IdHistorial { get; set; }

    public int IdNivel { get; set; }

    public int? PosicionAredl { get; set; }

    public decimal PuntosAredl { get; set; }

    public string EstadoAredl { get; set; } = null!;

    public DateTime Fecha { get; set; }

    public virtual Nivel IdNivelNavigation { get; set; } = null!;
}
