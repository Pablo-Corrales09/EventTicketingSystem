using API.Data;
using API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class LocalidadService
    {
        private readonly DbDevTicketappContext _context;

        // Constructor para inyectar el contexto de la base de datos en el servicio.
        public LocalidadService(DbDevTicketappContext context)
        {
            _context = context;
        }

        // Método para obtener todas las localidades de la base de datos y mapearlas a DTOs LocalidadSimpleDto.
        public async Task<List<LocalidadSimpleDto>> ObtenerLocalidadesAsync()
        {
            var localidades = await _context.Localidads
            .ToListAsync();

            return localidades.Select(l => MapearLocalidadDto(l))
            .ToList();
        }


        // Método para mapear una entidad Localidad a un DTO LocalidadSimpleDto.
        public static LocalidadSimpleDto MapearLocalidadDto(Models.Localidad l)
        {
            return new LocalidadSimpleDto
            {
                IdLocalidad = l.IdLocalidad,
                NombreLocalidad = l.NombreLocalidad,
                IdSedeEvento = l.IdSedeEvento
          
            };            
        }
 

    }// Fin de la clase.    

}// Fin del namespace.