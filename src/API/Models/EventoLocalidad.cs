using System;
using System.Collections.Generic;

namespace API.Models;

public partial class EventoLocalidad
{
    public int IdEventoLocalidad { get; set; }

    public int IdEvento { get; set; }

    public int IdLocalidad { get; set; }

    public decimal Precio { get; set; }

    public int CapacidadDisponible { get; set; }

    public virtual ICollection<Boleto> Boletos { get; set; } = new List<Boleto>();

    public virtual Evento IdEventoNavigation { get; set; } = null!;

    public virtual Localidad IdLocalidadNavigation { get; set; } = null!;
}
