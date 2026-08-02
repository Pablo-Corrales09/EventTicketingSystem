namespace API.Dtos
{
    public class BoletoActualizacionDto
    {
        public int IdBoleto { get; set; }

        public string NombreEvento { get; set; } = null!;
        public DateOnly? FechaEvento { get; set; }
        public TimeOnly? HoraEvento { get; set; }

        public int IdEventoLocalidad { get; set; }
        
        public string NombreLocalidad { get; set; } = null!;

        public decimal Precio { get; set; }
        //public int IdFactura { get; set; }

        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = null!;

        public string ApellidoUsuario { get; set; } = null!;

        public string CorreoUsuario { get; set; } = null!;

        public string TelefonoUsuario { get; set; } = null!;

        public int IdRol { get; set; }
        public string RolUsuario { get; set; } = null!;

        public string NumBoleto { get; set; } = null!;

        public DateTime? FechaCompra { get; set; }
    }
}