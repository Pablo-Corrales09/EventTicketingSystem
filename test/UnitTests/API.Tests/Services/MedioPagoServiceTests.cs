using API.Services;

namespace API.Tests.Services;

public class MedioPagoServiceTests
{
    [Fact]
    public async Task ObtenerMedioPagoPorIdAsync_CuandoElMedioDePagoExiste_DevuelveElDtoMapeado()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        contexto.MedioPagos.AddRange([DatosPrueba.CrearMedioPago()]);
        await contexto.SaveChangesAsync();
        var service = new MedioPagoService(contexto);

        // Act
        var resultado = await service.ObtenerMedioPagoPorIdAsync(1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.IdMedioPago.Should().Be(1);
        resultado.NombreMedioPago.Should().Be("Tarjeta de crédito");
    }

    [Fact]
    public async Task ObtenerMedioPagoPorIdAsync_CuandoElMedioDePagoNoExiste_DevuelveNull()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var service = new MedioPagoService(contexto);

        // Act
        var resultado = await service.ObtenerMedioPagoPorIdAsync(99);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerTodosLosMediosPagoAsync_CuandoHayDatos_DevuelveLaListaCompleta()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        contexto.MedioPagos.AddRange([
            DatosPrueba.CrearMedioPago(id: 1),
            DatosPrueba.CrearMedioPago(id: 2, nombre: "PSE"),
            DatosPrueba.CrearMedioPago(id: 3, nombre: "Efectivo")
        ]);
        await contexto.SaveChangesAsync();
        var service = new MedioPagoService(contexto);

        // Act
        var resultado = await service.ObtenerTodosLosMediosPagoAsync();

        // Assert
        resultado.Should().HaveCount(3);
        resultado.Should().Contain(m => m.NombreMedioPago == "PSE");
    }
}