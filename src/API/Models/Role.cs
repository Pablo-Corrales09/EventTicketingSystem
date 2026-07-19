using System;
using System.Collections.Generic;

namespace API.Models;

public partial class Role
{
    public int IdRole { get; set; }

    public string NombreRole { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
