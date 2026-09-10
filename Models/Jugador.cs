using System;
using System.Collections.Generic;

namespace UDL.Models;

public partial class Jugador
{
    public int IdJugador { get; set; }

    public string NombreGd { get; set; } = null!;

    public bool EsUruguayo { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaAlta { get; set; }

    public virtual ICollection<Record> Record { get; set; } = new List<Record>();

    public virtual ICollection<Submission> Submission { get; set; } = new List<Submission>();
}
