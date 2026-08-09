using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Evento
{
    public int IdEvento { get; set; }

    public string NombreEvento { get; set; } = null!;

    public DateTime FechaEvento { get; set; }

    public TimeOnly HoraEvento { get; set; }

    public int? IdSede { get; set; }

    //Agrega la propiedad ImageEvento para el manejo de las imagenes relacionadas a un evento.
    public string? ImageEvento { get; set; }

    public virtual ICollection<EventoLocalidad> EventoLocalidads { get; set; } = new List<EventoLocalidad>();

    public virtual SedeEvento? IdSedeNavigation { get; set; }
}
