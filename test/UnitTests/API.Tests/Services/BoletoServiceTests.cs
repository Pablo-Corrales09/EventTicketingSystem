using API.Services;

namespace API.Tests.Services;

public class BoletoServiceTests
{
    [Fact]
    public async Task ComprarBoleto_CuandoLaLocalidadDelEventoNoExiste_LanzaKeyNotFoundException()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var service = new BoletoService(contexto, new FacturaService(contexto));

        // Act
        var accion = async () => await service.ComprarBoleto(99, 1, 1, 1);

        // Assert
        await accion.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*no existe*");
    }

    [Fact]
    public async Task ComprarBoleto_CuandoNoHayDisponibilidadSuficiente_LanzaInvalidOperationException()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        await SembrarContextoCompletoAsync(contexto, capacidadDisponible: 2);
        var service = new BoletoService(contexto, new FacturaService(contexto));

        // Act
        var accion = async () => await service.ComprarBoleto(1, 1, 1, 5);

        // Assert
        await accion.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*disponibles*");
    }

    [Fact]
    public async Task ComprarBoleto_CuandoHayDisponibilidad_GeneraBoletosYDecrementaLaCapacidad()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        await SembrarContextoCompletoAsync(contexto, capacidadDisponible: 10, incluirBoleto: false);
        var service = new BoletoService(contexto, new FacturaService(contexto));

        // Act
        var resultado = await service.ComprarBoleto(1, 1, 1, 2);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.NumBoleto.Should().StartWith("BOL-CO-1-");
        resultado.NombreEvento.Should().Be("Concierto de Rock");
        resultado.Precio.Should().Be(100m);

        contexto.Boletos.Count().Should().Be(2);
        contexto.EventoLocalidads.Single().CapacidadDisponible.Should().Be(8);

        var factura = contexto.Facturas.Single();
        factura.NumeroFactura.Should().StartWith("FAC-");
        factura.Total.Should().Be(200m);
    }

    [Fact]
    public async Task ObtenerBoletoPorIdAsync_CuandoElBoletoExiste_DevuelveElDtoCompleto()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        await SembrarContextoCompletoAsync(contexto);
        var service = new BoletoService(contexto, new FacturaService(contexto));

        // Act
        var resultado = await service.ObtenerBoletoPorIdAsync(1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.IdBoleto.Should().Be(1);
        resultado.NombreUsuario.Should().Be("Ana");
        resultado.ApellidoUsuario.Should().Be("Pérez");
        resultado.NombreLocalidad.Should().Be("VIP");
        resultado.NombreEvento.Should().Be("Concierto de Rock");
    }

    [Fact]
    public async Task ObtenerBoletoPorIdAsync_CuandoElBoletoNoExiste_DevuelveNull()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var service = new BoletoService(contexto, new FacturaService(contexto));

        // Act
        var resultado = await service.ObtenerBoletoPorIdAsync(99);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task BuscarPorNumeroAsync_CuandoElNumeroNoExiste_DevuelveNull()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        await SembrarContextoCompletoAsync(contexto);
        var service = new BoletoService(contexto, new FacturaService(contexto));

        // Act
        var resultado = await service.BuscarPorNumeroAsync("BOL-NO-EXISTE");

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerTodosAsync_CuandoSeFiltraPorUsuario_DevuelveSoloLosBoletosDelUsuario()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        await SembrarContextoCompletoAsync(contexto);
        var service = new BoletoService(contexto, new FacturaService(contexto));

        // Act
        var resultado = await service.ObtenerTodosAsync(usuarioId: 1);

        // Assert
        resultado.Should().ContainSingle();
        resultado[0].CorreoUsuario.Should().Be("ana@correo.com");
    }

    private static async Task SembrarContextoCompletoAsync(
        ContextoPrueba contexto, int capacidadDisponible = 10, bool incluirBoleto = true)
    {
        var sede = DatosPrueba.CrearSedeEvento();
        var role = DatosPrueba.CrearRole();
        var usuario = DatosPrueba.CrearUsuario(role: role);
        var localidad = DatosPrueba.CrearLocalidad();
        var evento = DatosPrueba.CrearEvento(nombre: "Concierto de Rock");
        var eventoLocalidad = DatosPrueba.CrearEventoLocalidad(
            id: 1, idEvento: 1, idLocalidad: 1, precio: 100m, capacidadDisponible: capacidadDisponible);
        var medioPago = DatosPrueba.CrearMedioPago();

        contexto.SedeEventos.Add(sede);
        contexto.Roles.Add(role);
        contexto.Usuarios.Add(usuario);
        contexto.Localidads.Add(localidad);
        contexto.MedioPagos.Add(medioPago);
        contexto.Eventos.Add(evento);
        contexto.EventoLocalidads.Add(eventoLocalidad);

        if (incluirBoleto)
        {
            var factura = new Factura
            {
                IdUsuario = 1,
                IdMedioPago = 1,
                NumeroFactura = "FAC-2026-000001",
                Total = 100m
            };
            contexto.Facturas.Add(factura);
            contexto.Boletos.Add(new Boleto
            {
                IdEventoLocalidad = 1,
                IdUsuario = 1,
                IdFactura = 1,
                NumBoleto = "BOL-CO-1-1",
                FechaCompra = DateTime.Now
            });
        }

        await contexto.SaveChangesAsync();
    }
}