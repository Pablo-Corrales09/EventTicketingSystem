using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers;

public class EventosController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EventosController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CrearClienteApi()
    {
        return _httpClientFactory.CreateClient("API");
    }

    // 1. GET: /Eventos
    public async Task<IActionResult> Index()
    {
        var client = CrearClienteApi();

        try
        {
            var esAdmin = AuthCookieHelper.EsAdmin(Request);
            ViewBag.EsAdmin = esAdmin;

            List<EventoSedeViewModel> eventos;

            if (esAdmin)
            {
                ViewBag.Titulo = "Gestión de Eventos";
                eventos = await client.GetFromJsonAsync<List<EventoSedeViewModel>>("api/Eventos/localidad-sede")
                          ?? new List<EventoSedeViewModel>();
            }
            else
            {
                var sesion = AuthCookieHelper.ObtenerSesion(Request);

                if (sesion.Autenticado)
                {
                    ViewBag.Titulo = "Mis Eventos";
                    var query = QueryBuilder.Build(("usuarioId", sesion.IdUsuario));
                    eventos = await client.GetFromJsonAsync<List<EventoSedeViewModel>>($"api/Eventos/mis-eventos?{query}")
                              ?? new List<EventoSedeViewModel>();
                }
                else
                {
                    ViewBag.Titulo = "Eventos Disponibles";
                    eventos = await client.GetFromJsonAsync<List<EventoSedeViewModel>>("api/Eventos/localidad-sede")
                              ?? new List<EventoSedeViewModel>();
                }
            }

            return View(eventos);
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible conectarse con la API de eventos.";
            return View(new List<EventoSedeViewModel>());
        }
    }

    // 2. GET: /Eventos/ObtenerDetalles/5   
    [HttpGet]
    public async Task<IActionResult> ObtenerDetalles(int id)
    {
        try
        {
            var evento = await ObtenerEventoDetalleAsync(id);
            return evento == null ? NotFound("Evento no encontrado.") : Json(evento);
        }
        catch (HttpRequestException)
        {
            return StatusCode(500, "Error de conexión con la API al buscar los detalles.");
        }
    }

    // 3. GET: /Eventos/Details/5
    // Vista interactiva para seleccionar localidad, cantidad y comprar boletos.
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var evento = await ObtenerEventoDetalleAsync(id);

            if (evento == null)
            {
                return NotFound("Evento no encontrado.");
            }

            return View(evento);
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible conectarse con la API para obtener los detalles del evento.";
            return View(new EventoSedeViewModel());
        }
    }

    // 4. GET: /Eventos/Crear
    public async Task<IActionResult> Crear()
    {
        if (!AuthCookieHelper.EsAdmin(Request))
        {
            return RedirectToAction(nameof(Index));
        }

        var modelo = new EventoCreacionViewModel();
        await CargarSedesAsync(modelo);
        return View(modelo);
    }

    // 5. POST: /Eventos/Crear
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(EventoCreacionViewModel modelo)
    {
        if (!AuthCookieHelper.EsAdmin(Request))
        {
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            await CargarSedesAsync(modelo);
            return View(modelo);
        }

        var client = CrearClienteApi();

        try
        {
            using var contenido = new MultipartFormDataContent();

            contenido.Add(new StringContent(modelo.NombreEvento), "nombreEvento");
            contenido.Add(new StringContent(modelo.FechaEvento.ToString("yyyy-MM-ddTHH:mm:ss")), "fechaEvento");
            contenido.Add(new StringContent(modelo.HoraEvento.ToString("HH:mm:ss")), "horaEvento");
            contenido.Add(new StringContent(modelo.IdSede.ToString()), "idSede");

            if (modelo.ImagenEvento != null && modelo.ImagenEvento.Length > 0)
            {
                var archivo = new StreamContent(modelo.ImagenEvento.OpenReadStream());
                archivo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(modelo.ImagenEvento.ContentType);
                contenido.Add(archivo, "imagenEvento", modelo.ImagenEvento.FileName);
            }

            var respuesta = await client.PostAsync("api/Eventos/crear", contenido);

            if (respuesta.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "No se pudo crear el evento. Verifica los datos.");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "No fue posible conectarse con la API en este momento.");
        }

        await CargarSedesAsync(modelo);
        return View(modelo);
    }

    // 6. GET: /Eventos/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        if (!AuthCookieHelper.EsAdmin(Request))
        {
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var evento = await ObtenerEventoDetalleAsync(id);

            if (evento == null)
            {
                return NotFound("Evento no encontrado.");
            }

            var modelo = new EventoCreacionViewModel
            {
                IdEvento = evento.IdEvento,
                NombreEvento = evento.NombreEvento,
                FechaEvento = evento.FechaEvento,
                HoraEvento = TimeOnly.FromTimeSpan(evento.HoraEvento),
                IdSede = evento.Sede?.IdSedeEvento ?? 0,
                ImagenUrl = evento.ImagenUrl
            };

            await CargarSedesAsync(modelo);
            return View(modelo);
        }
        catch (HttpRequestException)
        {
            return RedirectToAction(nameof(Index));
        }
    }

    // 7. POST: /Eventos/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, EventoCreacionViewModel modelo)
    {
        if (!AuthCookieHelper.EsAdmin(Request))
        {
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            await CargarSedesAsync(modelo);
            return View(modelo);
        }

        var client = CrearClienteApi();

        try
        {
            using var contenido = new MultipartFormDataContent();

            contenido.Add(new StringContent(modelo.NombreEvento), "nombreEvento");
            contenido.Add(new StringContent(modelo.FechaEvento.ToString("yyyy-MM-ddTHH:mm:ss")), "fechaEvento");
            contenido.Add(new StringContent(modelo.HoraEvento.ToString("HH:mm:ss")), "horaEvento");
            contenido.Add(new StringContent(modelo.IdSede.ToString()), "idSede");

            if (modelo.ImagenEvento != null && modelo.ImagenEvento.Length > 0)
            {
                var archivo = new StreamContent(modelo.ImagenEvento.OpenReadStream());
                archivo.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(modelo.ImagenEvento.ContentType);
                contenido.Add(archivo, "imagenEvento", modelo.ImagenEvento.FileName);
            }

            var respuesta = await client.PutAsync($"api/Eventos/actualizar/{id}", contenido);

            if (respuesta.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "No se pudo actualizar el evento. Verifica los datos.");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "No fue posible conectarse con la API en este momento.");
        }

        await CargarSedesAsync(modelo);
        return View(modelo);
    }

    // Método auxiliar que obtiene el evento con su sede y las localidades asociadas.
    private async Task<EventoSedeViewModel?> ObtenerEventoDetalleAsync(int id)
    {
        var client = CrearClienteApi();
        var evento = await client.GetFromJsonAsync<EventoSedeViewModel>($"api/Eventos/{id}");

        if (evento == null)
        {
            return null;
        }

        try
        {
            var todasLasLocalidades = await client.GetFromJsonAsync<List<EventoLocalidadViewModel>>("api/EventosLocalidades");
            evento.Localidades = todasLasLocalidades?.Where(l => l.IdEvento == id).ToList() ?? new List<EventoLocalidadViewModel>();
        }
        catch (HttpRequestException)
        {
            evento.Localidades = new List<EventoLocalidadViewModel>();
        }

        return evento;
    }

    // Método auxiliar que carga las sedes para el desplegable del formulario.
    private async Task CargarSedesAsync(EventoCreacionViewModel modelo)
    {
        try
        {
            var client = CrearClienteApi();
            var sedes = await client.GetFromJsonAsync<List<SedeViewModel>>("api/Sedes");
            modelo.Sedes = sedes ?? new List<SedeViewModel>();
        }
        catch (HttpRequestException)
        {
            modelo.Sedes = new List<SedeViewModel>();
        }
    }
}
