namespace API.Dtos
{
    public class BoletoCreacionDto
    {
        public int IdBoleto { get; set; }
        public int IdEventoLocalidad { get; set; }
        public int IdFactura { get; set; }
        public int IdUsuario { get; set; }
        public int IdMedioPago { get; set; }
    }
}