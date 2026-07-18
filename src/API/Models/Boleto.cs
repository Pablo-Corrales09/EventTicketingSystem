using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Boleto
{
    public int IdBoleto { get; set; }

    public int IdEventoLocalidad { get; set; }

    public int IdFactura { get; set; }

    public int IdUsuario { get; set; }

    public string NumBoleto { get; set; } = null!;

    public DateTime? FechaCompra { get; set; }

    public virtual EventoLocalidad IdEventoLocalidadNavigation { get; set; } = null!;

    public virtual Factura IdFacturaNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
