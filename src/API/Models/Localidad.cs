using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Localidad
{
    public int IdLocalidad { get; set; }

    public string NombreLocalidad { get; set; } = null!;

    public int IdSedeEvento { get; set; }

    public virtual ICollection<EventoLocalidad> EventoLocalidads { get; set; } = new List<EventoLocalidad>();

    public virtual SedeEvento IdSedeEventoNavigation { get; set; } = null!;
}
