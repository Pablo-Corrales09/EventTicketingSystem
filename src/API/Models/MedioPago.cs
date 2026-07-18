using System;
using System.Collections.Generic;

namespace API.Models;

public partial class MedioPago
{
    public int IdMedioPago { get; set; }

    public string NombreMedioPago { get; set; } = null!;

    public virtual ICollection<Factura> Facturas { get; set; } = new List<Factura>();
}
