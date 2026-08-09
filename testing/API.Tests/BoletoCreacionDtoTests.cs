using API.Dtos;

namespace API.Tests;

public class BoletoCreacionDtoTests
{
    [Fact]
    public void CrearBoleto_ConDatosValidos_DebeConservarLosDatos()
    {
        // Arrange
        var boleto = new BoletoCreacionDto
        {
            IdBoleto = 1,
            IdEventoLocalidad = 4,
            IdFactura = 7,
            IdUsuario = 8,
            IdMedioPago = 1
        };

        // Act
        var idEventoLocalidad = boleto.IdEventoLocalidad;
        var idUsuario = boleto.IdUsuario;
        var idMedioPago = boleto.IdMedioPago;

        // Assert
        Assert.Equal(4, idEventoLocalidad);
        Assert.Equal(8, idUsuario);
        Assert.Equal(1, idMedioPago);
        Assert.Equal(7, boleto.IdFactura);
        Assert.Equal(1, boleto.IdBoleto);
    }
}