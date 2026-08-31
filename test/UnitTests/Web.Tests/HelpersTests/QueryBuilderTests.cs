using Web.Helpers;

namespace Web.Tests.HelpersTests;

public class QueryBuilderTests
{
    [Fact]
    public void Build_CuandoHayParametrosNullOVacios_LosOmite()
    {
        // Act
        var resultado = QueryBuilder.Build(
            ("numero", "123"),
            ("nombre", string.Empty),
            ("nulo", null),
            ("otro", "  "));

        // Assert
        resultado.Should().Be("numero=123");
    }

    [Fact]
    public void Build_CuandoNoHayParametrosValidos_DevuelveCadenaVacia()
    {
        // Act
        var resultado = QueryBuilder.Build(("a", null), ("b", ""));

        // Assert
        resultado.Should().BeEmpty();
    }

    [Fact]
    public void Build_CuandoHayDateOnly_UsaElFormatoISODeFecha()
    {
        // Act
        var resultado = QueryBuilder.Build(("fecha", new DateOnly(2026, 8, 16)));

        // Assert
        resultado.Should().Be("fecha=2026-08-16");
    }

    [Fact]
    public void Build_CuandoHayDateTime_UsaElFormatoISOConHora()
    {
        // Act
        var resultado = QueryBuilder.Build(("fecha", new DateTime(2026, 8, 16, 14, 30, 0)));

        // Assert
        resultado.Should().Be("fecha=2026-08-16T14:30:00");
    }

    [Fact]
    public void Build_CuandoHayTextoConCaracteresEspeciales_LoEscapa()
    {
        // Act
        var resultado = QueryBuilder.Build(("nombre", "Rock & Roll"));

        // Assert
        resultado.Should().Be("nombre=Rock%20%26%20Roll");
    }
}