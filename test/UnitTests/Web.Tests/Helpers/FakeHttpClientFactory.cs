namespace Web.Tests.Helpers;

// Construye el IHttpClientFactory (mockeado) cuyo cliente "API" usa el handler fake.
public static class FakeHttpClientFactory
{
    private const string BaseUrl = "http://localhost:5123/";

    public static (IHttpClientFactory Factory, FakeHttpHandler Handler) Crear(
        Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new FakeHttpHandler(responder);
        var factory = new Mock<IHttpClientFactory>();
        factory
            .Setup(f => f.CreateClient("API"))
            .Returns(new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) });
        return (factory.Object, handler);
    }
}