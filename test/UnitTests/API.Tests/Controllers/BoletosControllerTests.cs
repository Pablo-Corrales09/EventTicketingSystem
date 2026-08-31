using API.Controllers;
using API.Dtos;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Tests.Controllers;

public class BoletosControllerTests
{
    [Fact]
    public async Task GetBoleto_CuandoElBoletoExiste_DevuelveOkConElDto()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        await SembrarBoletoAsync(contexto);
        var controller = new BoletosController(new BoletoService(contexto, new FacturaService(contexto)));

        // Act
        var resultado = await controller.GetBoleto(1);

        // Assert
        var ok = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dto = ok.Value.Should().BeOfType<BoletoDto>().Subject;
        dto.IdBoleto.Should().Be(1);
        dto.NombreEvento.Should().Be("Concierto de Rock");
    }

    [Fact]
    public async Task GetBoleto_CuandoElBoletoNoExiste_DevuelveNotFound()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var controller = new BoletosController(new BoletoService(contexto, new FacturaService(contexto)));

        // Act
        var resultado = await controller.GetBoleto(99);

        // Assert
        resultado.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetBoletos_CuandoHayBoletos_DevuelveOkConLaLista()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        await SembrarBoletoAsync(contexto);
        var controller = new BoletosController(new BoletoService(contexto, new FacturaService(contexto)));

        // Act
        var resultado = await controller.GetBoletos();

        // Assert
        var ok = resultado.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<BoletoDto>>().Subject.Should().ContainSingle();
    }

    [Fact]
    public async Task BuscarPorNumero_CuandoElNumeroNoExiste_DevuelveNotFound()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        await SembrarBoletoAsync(contexto);
        var controller = new BoletosController(new BoletoService(contexto, new FacturaService(contexto)));

        // Act
        var resultado = await controller.BuscarPorNumero("BOL-NO-EXISTE");

        // Assert
        resultado.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CompraDeBoleto_CuandoLaCompraEsExitosa_DevuelveCreatedAtAction()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        await SembrarBoletoAsync(contexto, capacidadDisponible: 10);
        var controller = new BoletosController(new BoletoService(contexto, new FacturaService(contexto)));
        var request = new BoletoCreacionDto
        {
            IdEventoLocalidad = 1,
            IdUsuario = 1,
            IdMedioPago = 1,
            Cantidad = 2
        };

        // Act
        var resultado = await controller.CompraDeBoleto(request);

        // Assert
        var created = resultado.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(BoletosController.GetBoleto));
        created.Value.Should().BeOfType<BoletoDto>();
    }

    private static async Task SembrarBoletoAsync(ContextoPrueba contexto, int capacidadDisponible = 5)
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

        await contexto.SaveChangesAsync();
    }
}