using System.Globalization;

namespace Web.Helpers;

public static class QueryBuilder
{
    public static string Build(params (string Nombre, object? Valor)[] parametros)
    {
        var partes = new List<string>();

        foreach (var (nombre, valor) in parametros)
        {
            if (valor == null)
            {
                continue;
            }

            if (valor is string texto)
            {
                if (string.IsNullOrWhiteSpace(texto))
                {
                    continue;
                }
                partes.Add($"{Uri.EscapeDataString(nombre)}={Uri.EscapeDataString(texto)}");
                continue;
            }

            if (valor is DateOnly dateOnly)
            {
                partes.Add($"{Uri.EscapeDataString(nombre)}={dateOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
                continue;
            }

            if (valor is DateTime fecha)
            {
                partes.Add($"{Uri.EscapeDataString(nombre)}={fecha.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)}");
                continue;
            }

            partes.Add($"{Uri.EscapeDataString(nombre)}={Uri.EscapeDataString(Convert.ToString(valor, CultureInfo.InvariantCulture) ?? string.Empty)}");
        }

        return string.Join("&", partes);
    }
}
