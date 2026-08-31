using Microsoft.Data.Sqlite;

namespace API.Tests.Helpers;

// Fixture que crea un DbDevTicketappContext real sobre SQLite en memoria (:memory:),
// soporta transacciones (a diferencia del proveedor InMemory de EF Core) y aísla cada
// prueba con una conexión propia.
public sealed class ContextoPrueba(SqliteConnection conexion)
    : DbDevTicketappContext(new DbContextOptionsBuilder<DbDevTicketappContext>().UseSqlite(conexion).Options),
      IDisposable
{
    public static async Task<ContextoPrueba> CrearAsync()
    {
        var conexion = new SqliteConnection("DataSource=:memory:");
        await conexion.OpenAsync();
        var contexto = new ContextoPrueba(conexion);
        await contexto.Database.EnsureCreatedAsync();
        return contexto;
    }

    // SQLite no acepta DEFAULT (getdate()) (sintaxis de SQL Server) al crear el esquema,
    // así que se eliminan los valores por defecto solo en el modelo de pruebas.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Boleto>().Property(b => b.FechaCompra).HasDefaultValueSql(null);
        modelBuilder.Entity<Factura>().Property(f => f.FechaFactura).HasDefaultValueSql(null);
        modelBuilder.Entity<Usuario>().Property(u => u.FechaCreacion).HasDefaultValueSql(null);
    }

    // El OnConfiguring base registra UseSqlServer, lo que entraría en conflicto con
    // el provider SQLite inyectado por constructor. Aquí se omite esa configuración.
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    public override void Dispose()
    {
        conexion.Dispose();
        base.Dispose();
    }
}