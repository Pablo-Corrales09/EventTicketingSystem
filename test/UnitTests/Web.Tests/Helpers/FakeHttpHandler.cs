using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Web.Tests.Helpers;

// Handler HTTP fake que devuelve respuestas controladas por ruta/petición y
// registra cada solicitud para poder verificar URLs, query strings y headers.
public sealed class FakeHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    : HttpMessageHandler
{
    public List<HttpRequestMessage> Solicitudes { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Solicitudes.Add(request);
        return Task.FromResult(responder(request));
    }

    public static HttpResponseMessage Json(object valor, HttpStatusCode codigo = HttpStatusCode.OK) =>
        new(codigo)
        {
            Content = JsonContent.Create(valor, options: new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        };

    public static HttpResponseMessage Error(string mensaje, HttpStatusCode codigo = HttpStatusCode.BadRequest) =>
        new(codigo)
        {
            Content = JsonContent.Create(new { mensaje }, options: new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        };
}