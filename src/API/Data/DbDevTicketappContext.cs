using System;
using System.Collections.Generic;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public partial class DbDevTicketappContext : DbContext
{
    public DbDevTicketappContext()
    {
    }

    public DbDevTicketappContext(DbContextOptions<DbDevTicketappContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Boleto> Boletos { get; set; }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<EventoLocalidad> EventoLocalidads { get; set; }

    public virtual DbSet<Factura> Facturas { get; set; }

    public virtual DbSet<Localidad> Localidads { get; set; }

    public virtual DbSet<MedioPago> MedioPagos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SedeEvento> SedeEventos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Boleto>(entity =>
        {
            entity.HasKey(e => e.IdBoleto);

            entity.ToTable("boleto");

            entity.HasIndex(e => e.NumBoleto, "UQ__boleto__5F58F54E312BB92F").IsUnique();

            entity.Property(e => e.IdBoleto).HasColumnName("id_boleto");
            entity.Property(e => e.FechaCompra)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_compra");
            entity.Property(e => e.IdEventoLocalidad).HasColumnName("id_evento_localidad");
            entity.Property(e => e.IdFactura).HasColumnName("id_factura");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.NumBoleto)
                .HasMaxLength(50)
                .HasColumnName("num_boleto");

            entity.HasOne(d => d.IdEventoLocalidadNavigation).WithMany(p => p.Boletos)
                .HasForeignKey(d => d.IdEventoLocalidad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_evento_localidad");

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.Boletos)
                .HasForeignKey(d => d.IdFactura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_factura");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Boletos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_usuario");
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.IdEvento);

            entity.ToTable("evento");

            entity.Property(e => e.IdEvento).HasColumnName("id_evento");
            entity.Property(e => e.FechaEvento)
                .HasColumnType("datetime")
                .HasColumnName("fecha_evento");
            entity.Property(e => e.HoraEvento).HasColumnName("hora_evento");
            entity.Property(e => e.ImageEvento)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("image_evento");
            entity.Property(e => e.NombreEvento)
                .HasMaxLength(100)
                .HasColumnName("nombre_evento");

            entity.HasOne(d => d.IdSedeNavigation).WithMany(p => p.Eventos)
                .HasForeignKey(d => d.IdSede)
                .HasConstraintName("FK_evento_sede_evento");
        });

        modelBuilder.Entity<EventoLocalidad>(entity =>
        {
            entity.HasKey(e => e.IdEventoLocalidad);

            entity.ToTable("evento_localidad");

            entity.Property(e => e.IdEventoLocalidad).HasColumnName("id_evento_localidad");
            entity.Property(e => e.CapacidadDisponible).HasColumnName("capacidad_disponible");
            entity.Property(e => e.IdEvento).HasColumnName("id_evento");
            entity.Property(e => e.IdLocalidad).HasColumnName("id_localidad");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.EventoLocalidads)
                .HasForeignKey(d => d.IdEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_evento");

            entity.HasOne(d => d.IdLocalidadNavigation).WithMany(p => p.EventoLocalidads)
                .HasForeignKey(d => d.IdLocalidad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_localidad");
        });

        modelBuilder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.IdFactura);

            entity.ToTable("factura");

            entity.HasIndex(e => e.NumeroFactura, "UQ__factura__3DC4B241A96C28B6").IsUnique();

            entity.Property(e => e.IdFactura).HasColumnName("id_factura");
            entity.Property(e => e.FechaFactura)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_factura");
            entity.Property(e => e.IdMedioPago).HasColumnName("id_medio_pago");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.NumeroFactura)
                .HasMaxLength(50)
                .HasColumnName("numero_factura");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.IdMedioPagoNavigation).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.IdMedioPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_factura_medio_pago");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Facturas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_factura_usuario");
        });

        modelBuilder.Entity<Localidad>(entity =>
        {
            entity.HasKey(e => e.IdLocalidad);

            entity.ToTable("localidad");

            entity.Property(e => e.IdLocalidad).HasColumnName("id_localidad");
            entity.Property(e => e.IdSedeEvento).HasColumnName("id_sede_evento");
            entity.Property(e => e.NombreLocalidad)
                .HasMaxLength(100)
                .HasColumnName("nombre_localidad");

            entity.HasOne(d => d.IdSedeEventoNavigation).WithMany(p => p.Localidads)
                .HasForeignKey(d => d.IdSedeEvento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_localidad_sede_evento");
        });

        modelBuilder.Entity<MedioPago>(entity =>
        {
            entity.HasKey(e => e.IdMedioPago);

            entity.ToTable("medio_pago");

            entity.Property(e => e.IdMedioPago).HasColumnName("id_medio_pago");
            entity.Property(e => e.NombreMedioPago)
                .HasMaxLength(50)
                .HasColumnName("nombre_medio_pago");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole);

            entity.ToTable("role");

            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.NombreRole)
                .HasMaxLength(50)
                .HasColumnName("nombre_role");
        });

        modelBuilder.Entity<SedeEvento>(entity =>
        {
            entity.HasKey(e => e.IdSedeEvento);

            entity.ToTable("sede_evento");

            entity.Property(e => e.IdSedeEvento).HasColumnName("id_sede_evento");
            entity.Property(e => e.NombreSedeEvento)
                .HasMaxLength(50)
                .HasColumnName("nombre_sede_evento");
            entity.Property(e => e.Ubicacion)
                .HasMaxLength(100)
                .HasColumnName("ubicacion");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);

            entity.ToTable("usuario");

            entity.HasIndex(e => e.Correo, "UQ__usuario__2A586E0B492B5F82").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .HasColumnName("apellido");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasDefaultValue("")
                .HasColumnName("password_hash");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRole)
                .HasConstraintName("FK_usuario_role");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
