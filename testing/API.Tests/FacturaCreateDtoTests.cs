using API.DTOs;

namespace API.Tests;

public class FacturaCreateDtoTests
{
    [Fact]
    public void CrearFactura_ConDatosValidos_DebeConservarLosDatos()
    {
        // Arrange
        var factura = new FacturaCreateDto
        {
            IdUsuario = 8,
            IdMedioPago = 1,
            IdsBoletos = new List<int> { 4 }
        };

        // Act
        var idUsuario = factura.IdUsuario;
        var idMedioPago = factura.IdMedioPago;
        var boletos = factura.IdsBoletos;

        // Assert
        Assert.Equal(8, idUsuario);
        Assert.Equal(1, idMedioPago);
        Assert.Single(boletos);
        Assert.Equal(4, boletos[0]);
    }
}