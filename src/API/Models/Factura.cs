using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Factura
{
    public int IdFactura { get; set; }

    public int IdUsuario { get; set; }

    public int IdMedioPago { get; set; }

    public DateTime? FechaFactura { get; set; }

    public string NumeroFactura { get; set; } = null!;

    public decimal Total { get; set; }

    public virtual ICollection<Boleto> Boletos { get; set; } = new List<Boleto>();

    public virtual MedioPago IdMedioPagoNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
