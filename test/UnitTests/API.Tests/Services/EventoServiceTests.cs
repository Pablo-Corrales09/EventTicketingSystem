using API.Dtos;
using API.Services;
using Microsoft.Extensions.Configuration;

namespace API.Tests.Services;

public class EventoServiceTests
{
    private static readonly IConfiguration ConfiguracionVacia =
        new ConfigurationBuilder().Build();

    [Fact]
    public async Task ObtenerTodosLosEventosAsync_CuandoHayEventos_DevuelveLosDtosSinSede()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var sede = DatosPrueba.CrearSedeEvento();
        contexto.SedeEventos.Add(sede);
        contexto.Eventos.Add(DatosPrueba.CrearEvento(nombre: "Festival de Jazz"));
        await contexto.SaveChangesAsync();
        var service = new EventoService(contexto, ConfiguracionVacia);

        // Act
        var resultado = await service.ObtenerTodosLosEventosAsync();

        // Assert
        resultado.Should().ContainSingle();
        resultado[0].NombreEvento.Should().Be("Festival de Jazz");
        resultado[0].ImagenUrl.Should().BeNull();
    }

    [Fact]
    public async Task ObtenerEventoSedePorIdAsync_CuandoElEventoExiste_DevuelveElDtoConLaSede()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var sede = DatosPrueba.CrearSedeEvento();
        contexto.SedeEventos.Add(sede);
        contexto.Eventos.Add(DatosPrueba.CrearEvento());
        await contexto.SaveChangesAsync();
        var service = new EventoService(contexto, ConfiguracionVacia);

        // Act
        var resultado = await service.ObtenerEventoSedePorIdAsync(1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Sede.Should().NotBeNull();
        resultado.Sede!.NombreSedeEvento.Should().Be("Coliseo Central");
        resultado.Sede.Ubicacion.Should().Be("Av. Principal 123");
    }

    [Fact]
    public async Task ObtenerEventoSedePorIdAsync_CuandoElEventoNoExiste_DevuelveNull()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var service = new EventoService(contexto, ConfiguracionVacia);

        // Act
        var resultado = await service.ObtenerEventoSedePorIdAsync(99);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ActualizarEventoAsync_CuandoElEventoNoExiste_DevuelveNull()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var service = new EventoService(contexto, ConfiguracionVacia);

        // Act
        var resultado = await service.ActualizarEventoAsync(
            99, new EventoCreacionDto { NombreEvento = "Nuevo nombre" });

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ActualizarEventoAsync_CuandoElEventoExiste_ActualizaLosDatosDelEvento()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var sede = DatosPrueba.CrearSedeEvento();
        contexto.SedeEventos.Add(sede);
        contexto.Eventos.Add(DatosPrueba.CrearEvento());
        await contexto.SaveChangesAsync();
        var service = new EventoService(contexto, ConfiguracionVacia);

        // Act
        var resultado = await service.ActualizarEventoAsync(
            1, new EventoCreacionDto { NombreEvento = "Concierto Renombrado", IdSede = 1 });

        // Assert
        resultado.Should().NotBeNull();
        resultado!.NombreEvento.Should().Be("Concierto Renombrado");
    }

    [Fact]
    public async Task ObtenerEventosDeUsuarioAsync_CuandoElUsuarioTieneBoletos_DevuelveSoloSusEventos()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var sede = DatosPrueba.CrearSedeEvento();
        var usuario = DatosPrueba.CrearUsuario();
        var eventoConBoleto = DatosPrueba.CrearEvento(id: 1, nombre: "Concierto de Rock");
        var eventoSinBoleto = DatosPrueba.CrearEvento(id: 2, nombre: "Festival de Jazz");
        var localidad = DatosPrueba.CrearLocalidad();
        var eventoLocalidad = DatosPrueba.CrearEventoLocalidad(id: 1, idEvento: 1, idLocalidad: 1);
        var medioPago = DatosPrueba.CrearMedioPago();

        contexto.SedeEventos.Add(sede);
        contexto.Usuarios.Add(usuario);
        contexto.Localidads.Add(localidad);
        contexto.MedioPagos.Add(medioPago);
        contexto.Eventos.AddRange([eventoConBoleto, eventoSinBoleto]);
        contexto.EventoLocalidads.Add(eventoLocalidad);

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
            NumBoleto = "BOL-CO-1-1"
        });
        await contexto.SaveChangesAsync();

        var service = new EventoService(contexto, ConfiguracionVacia);

        // Act
        var resultado = await service.ObtenerEventosDeUsuarioAsync(1);

        // Assert
        resultado.Should().ContainSingle();
        resultado[0].NombreEvento.Should().Be("Concierto de Rock");
    }
}