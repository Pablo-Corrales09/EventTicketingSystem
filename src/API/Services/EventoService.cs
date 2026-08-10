using API.Data;
using API.Dtos;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using API.Models;

namespace API.Services
{
    public class EventoService
    {
        private readonly DbDevTicketappContext _context;
        private readonly IConfiguration _configuration;

        public EventoService(DbDevTicketappContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }    

        // Función para obtener los eventos de la DB y mapearlos a un EventoDto, sin incluir la información de las localidades asociadas al evento.
        public async Task<List<EventoDto>> ObtenerTodosLosEventosAsync()
        {
            var eventos = await _context.Eventos
                .ToListAsync();
            return eventos.Select(e => MapearEventoLocalidadDto(e)).ToList();
        }

        //Funcion para obtener un evento por su id
        public async Task<EventoSedeDto?> ObtenerDetalleEventoAsync(int id)
        {
            return await _context.Eventos
            .Include(e => e.IdSedeNavigation)
            .Where(e => e.IdEvento == id)
            .Select(e => MapearEventoSedeDto(e))
            .FirstOrDefaultAsync();    
        }

        // Método para crear un nuevo evento en la DB subiendo la imagen a Azure
        public async Task<EventoDto> CrearEventoAsync(EventoCreacionDto eventoCreacionDto)
        {
            string? urlImagenGuardada = null;

            // Verifica si el archivo viene en el DTO
            if(eventoCreacionDto.ImagenEvento != null && eventoCreacionDto.ImagenEvento.Length > 0)
            {
                string connectionString = _configuration.GetConnectionString("AzureStorage")!;
                string containerName = "eventos-img";

                //Conexion con Azure Blob Storage
                var blobServiceClient = new BlobServiceClient(connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

                //Genera un nombre único para el archivo
                var extension = Path.GetExtension(eventoCreacionDto.ImagenEvento.FileName);
                var nombreImagen = Guid.NewGuid().ToString() + extension;

                // Prepara el cliente del archivo y lo sube desde la memoria
                var blobClient = blobContainerClient.GetBlobClient(nombreImagen);
                
                using (var stream = eventoCreacionDto.ImagenEvento.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                //Genera la URL pública generada por Azure
                urlImagenGuardada = blobClient.Uri.ToString();
            }

            var nuevoEvento = new Models.Evento
            {
                NombreEvento = eventoCreacionDto.NombreEvento,
                FechaEvento = eventoCreacionDto.FechaEvento,
                HoraEvento = eventoCreacionDto.HoraEvento,
                IdSede = eventoCreacionDto.IdSede,
                ImageEvento = urlImagenGuardada
            };

            _context.Eventos.Add(nuevoEvento);
            await _context.SaveChangesAsync();

            return MapearEventoLocalidadDto(nuevoEvento);
        }

        
        // Método para actualizar un evento existente
        public async Task<EventoDto?> ActualizarEventoAsync(int id, EventoCreacionDto eventoActualizacionDto)
        {
            //Busca el evento en la base de datos
            var eventoExistente = await _context.Eventos.FindAsync(id);
            if (eventoExistente == null)
            {
                return null;
            }

            //Verifica si el usuario envió una imagen nueva para actualizarla
            if(eventoActualizacionDto.ImagenEvento != null && eventoActualizacionDto.ImagenEvento.Length > 0)
            {
                string connectionString = _configuration.GetConnectionString("AzureStorage")!;
                string containerName = "eventos-img"; 

                var blobServiceClient = new BlobServiceClient(connectionString);
                var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

                var extension = Path.GetExtension(eventoActualizacionDto.ImagenEvento.FileName);
                var nombreImagen = Guid.NewGuid().ToString() + extension;

                var blobClient = blobContainerClient.GetBlobClient(nombreImagen);
                
                using (var stream = eventoActualizacionDto.ImagenEvento.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                eventoExistente.ImageEvento = blobClient.Uri.ToString();
            }
            eventoExistente.NombreEvento = eventoActualizacionDto.NombreEvento;
            eventoExistente.FechaEvento = eventoActualizacionDto.FechaEvento;
            eventoExistente.HoraEvento = eventoActualizacionDto.HoraEvento;
            eventoExistente.IdSede = eventoActualizacionDto.IdSede;

            await _context.SaveChangesAsync();

            return MapearEventoLocalidadDto(eventoExistente);
        }
        
        // Método auxiliar para evitar la duplicación de código al mapear un objeto Evento a EventoDto
        private static EventoDto MapearEventoLocalidadDto(Models.Evento e)
        {
            return new EventoDto
            {
                IdEvento = e.IdEvento,
                NombreEvento = e.NombreEvento,
                FechaEvento = e.FechaEvento,
                HoraEvento = e.HoraEvento,
                ImagenUrl = e.ImageEvento 
            };
        }

        // Función para obtener los eventos en los que el usuario tiene boletos comprados ("mis eventos"),
        // incluyendo la información de la sede asociada a cada evento.
        public async Task<List<EventoSedeDto>> ObtenerEventosDeUsuarioAsync(int usuarioId)
        {
            var eventos = await _context.Eventos
                .Include(e => e.IdSedeNavigation)
                .Where(e => e.EventoLocalidads.Any(el =>
                    el.Boletos.Any(b => b.IdUsuario == usuarioId)))
                .ToListAsync();

            return eventos.Select(e => MapearEventoSedeDto(e)).ToList();
        }

        // Función para obtener los eventos de la DB y mapearlos a un EventoSedeDto, incluyendo la información de la sede asociada al evento.
        public async Task<List<EventoSedeDto>> ObtenerEventosSedeAsync()
        {
            var eventos = await _context.Eventos
                .Include(e => e.IdSedeNavigation)
                .ToListAsync();
            return eventos.Select(e => MapearEventoSedeDto(e)).ToList();
        }

        // Función para obtener un evento por su ID de la DB y mapearlo a un EventoSedeDto, incluyendo la información de la sede asociada al evento.
        public async Task<EventoSedeDto?> ObtenerEventoSedePorIdAsync(int id)
        {
            var evento = await _context.Eventos
                .Where(e => e.IdEvento == id)
                .Include(e => e.IdSedeNavigation)
                .FirstOrDefaultAsync();

            // Si el evento existe se mapea al DTO. Si no, devolvemos null.
            return evento != null ? MapearEventoSedeDto(evento) : null;
        }

        // Método auxiliar para evitar la duplicación de código al mapear un objeto Evento a EventoSedeDto
        private static EventoSedeDto MapearEventoSedeDto(Models.Evento e)
        {
            return new EventoSedeDto
            {
                IdEvento = e.IdEvento,
                NombreEvento = e.NombreEvento,
                FechaEvento = e.FechaEvento,
                HoraEvento = e.HoraEvento,
                ImagenUrl = e.ImageEvento, 
                Sede = e.IdSedeNavigation != null ? new SedeUbicacionSimpleDto
                {
                    IdSedeEvento = e.IdSedeNavigation.IdSedeEvento,
                    NombreSedeEvento = e.IdSedeNavigation.NombreSedeEvento,
                    Ubicacion = e.IdSedeNavigation.Ubicacion
                } : null,
            };
        }
    }
}