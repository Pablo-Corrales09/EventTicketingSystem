namespace API.Dtos
{
    public class BoletoBaseDto
    {
        public int IdBoleto { get; set; }

        public int IdEventoLocalidad { get; set; }

        public int IdFactura { get; set; }

        public int IdUsuario { get; set; }

        public string NumBoleto { get; set; } = null!;

        public DateTime? FechaCompra { get; set; }

    }
}