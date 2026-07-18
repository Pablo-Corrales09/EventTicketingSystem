using System;
using System.Collections.Generic;

namespace API.Models;

public partial class SedeEvento
{
    public int IdSedeEvento { get; set; }

    public string NombreSedeEvento { get; set; } = null!;

    public string Ubicacion { get; set; } = null!;

    public virtual ICollection<Localidad> Localidads { get; set; } = new List<Localidad>();
}
