namespace API.Dtos
{
    public class BoletoDto
    {
        // 1. Datos del boleto
        public int IdBoleto { get; set; }
        public string NumBoleto { get; set; } = null!;
        public DateOnly? FechaCompra { get; set; }

        // 2. Datos de facturación y precio
        public string NumeroFactura { get; set; } = null!;
        public decimal Precio { get; set; }

        // 3. Datos del evento
        public string NombreEvento { get; set; } = null!;
        public DateOnly? FechaEvento { get; set; }
        public TimeOnly? HoraEvento { get; set; }
        public string NombreLocalidad { get; set; } = null!;

        // 4. Datos del usuario
        public string NombreUsuario { get; set; } = null!;
        public string ApellidoUsuario { get; set; } = null!;
        public string CorreoUsuario { get; set; } = null!;
        public string TelefonoUsuario { get; set; } = null!;
    }
}