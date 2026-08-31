using API.Controllers;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace API.Tests.Controllers;

public class EventosControllerTests
{
    private static readonly IConfiguration ConfiguracionVacia =
        new ConfigurationBuilder().Build();

    [Fact]
    public async Task GetEventos_CuandoHayEventos_DevuelveOkConLaLista()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        contexto.SedeEventos.Add(DatosPrueba.CrearSedeEvento());
        contexto.Eventos.Add(DatosPrueba.CrearEvento(nombre: "Festival de Jazz"));
        await contexto.SaveChangesAsync();
        var controller = new EventosController(new EventoService(contexto, ConfiguracionVacia));

        // Act
        var resultado = await controller.GetEventos();

        // Assert
        var ok = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<API.Dtos.EventoDto>>().Subject.Should().ContainSingle();
    }

    [Fact]
    public async Task GetEventoSede_CuandoElEventoExiste_DevuelveOk()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var sede = DatosPrueba.CrearSedeEvento();
        contexto.SedeEventos.Add(sede);
        contexto.Eventos.Add(DatosPrueba.CrearEvento());
        await contexto.SaveChangesAsync();
        var controller = new EventosController(new EventoService(contexto, ConfiguracionVacia));

        // Act
        var resultado = await controller.GetEventoSede(1);

        // Assert
        var ok = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<API.Dtos.EventoSedeDto>().Subject;
        dto.Sede.Should().NotBeNull();
        dto.Sede!.NombreSedeEvento.Should().Be("Coliseo Central");
    }

    [Fact]
    public async Task GetEventoSede_CuandoElEventoNoExiste_DevuelveNotFound()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var controller = new EventosController(new EventoService(contexto, ConfiguracionVacia));

        // Act
        var resultado = await controller.GetEventoSede(99);

        // Assert
        resultado.Result.Should().BeOfType<NotFoundResult>();
    }
}