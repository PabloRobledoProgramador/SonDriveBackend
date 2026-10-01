using Biblioteca.Modelo;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Conexion
{
    public class Conexion : DbContext, IConexion
    {
        // Inyecta las opciones desde Program.cs 
        public Conexion(DbContextOptions<Conexion> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Puesto> Puestos { get; set; }
        public DbSet<Viaje> Viajes { get; set; }
        public DbSet<Ocupacion> Ocupaciones { get; set; }
        public DbSet<Ruta> Rutas { get; set; }
        public DbSet<Ciudad> Ciudades { get; set; }
        public DbSet<Hora> Horas { get; set; }
        public DbSet<Falla> Fallas { get; set; }

        
        public DbSet<Pasajero> Pasajeros { get; set; }
        public DbSet<TipoPuesto> TiposPuesto { get; set; }
        public DbSet<CodigoPuesto> CodigosPuesto { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===================== USUARIO =====================
            modelBuilder.Entity<Usuario>(e =>
            {
                e.ToTable("Usuarios");
                e.HasKey(x => x.Id);

                e.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
                e.Property(x => x.Apellido).IsRequired().HasMaxLength(100);
                e.Property(x => x.Correo).IsRequired().HasMaxLength(150);
                e.Property(x => x.Clave).IsRequired().HasMaxLength(255); // hash de la clave
                e.Property(x => x.Celular).IsRequired().HasMaxLength(20);
                e.Property(x => x.Estado).IsRequired();
                e.Property(x => x.Rol).IsRequired().HasMaxLength(30).HasDefaultValue("Conductor");
                e.Property(x => x.Foto).HasMaxLength(500); // opcional (ruta/URL)

                e.HasIndex(x => x.Correo).IsUnique();

                // 1:N Usuario -> Vehiculos
                e.HasMany(x => x.Vehiculos)
                 .WithOne(v => v.Usuario)
                 .HasForeignKey(v => v.IdUsuario)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== VEHICULO =====================
            modelBuilder.Entity<Vehiculo>(e =>
            {
                e.ToTable("Vehiculos");
                e.HasKey(x => x.Id);

                e.Property(x => x.Placa).IsRequired().HasMaxLength(10);
                e.Property(x => x.Modelo).IsRequired().HasMaxLength(100);
                e.Property(x => x.AnnoModelo).IsRequired().HasMaxLength(4);
                e.Property(x => x.Tipo).IsRequired().HasMaxLength(50);

                e.HasIndex(x => x.Placa).IsUnique();

                // Vehiculo -> Puestos (los puestos no existen sin el vehículo)
                e.HasMany(x => x.Puestos)
                 .WithOne(p => p.Vehiculo)
                 .HasForeignKey(p => p.IdVehiculo)
                 .OnDelete(DeleteBehavior.Cascade);

                // Vehiculo -> Viajes (se conserva el historial)
                e.HasMany(x => x.Viajes)
                 .WithOne(v => v.Vehiculo)
                 .HasForeignKey(v => v.IdVehiculo)
                 .OnDelete(DeleteBehavior.Restrict);

                // Vehiculo -> Fallas
                e.HasMany(x => x.Fallas)
                 .WithOne(f => f.Vehiculo)
                 .HasForeignKey(f => f.VehiculoId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================== TIPO PUESTO =====================
            modelBuilder.Entity<TipoPuesto>(e =>
            {
                e.ToTable("TiposPuesto");
                e.HasKey(x => x.Id);
                e.Property(x => x.Descripcion).IsRequired().HasMaxLength(50);
                e.HasIndex(x => x.Descripcion).IsUnique();

                e.HasMany(x => x.Puestos)
                 .WithOne(p => p.TipoPuesto)
                 .HasForeignKey(p => p.IdTipo)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== CODIGO PUESTO =====================
            modelBuilder.Entity<CodigoPuesto>(e =>
            {
                e.ToTable("CodigosPuesto");
                e.HasKey(x => x.Id);
                e.Property(x => x.Descripcion).IsRequired().HasMaxLength(50);
                e.HasIndex(x => x.Descripcion).IsUnique();

                e.HasMany(x => x.Puestos)
                 .WithOne(p => p.CodigoPuesto)
                 .HasForeignKey(p => p.IdCodigo)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== PUESTO =====================
            modelBuilder.Entity<Puesto>(e =>
            {
                e.ToTable("Puestos");
                e.HasKey(x => x.Id);

                // Evita duplicar el mismo código de puesto dentro de un vehículo
                e.HasIndex(x => new { x.IdVehiculo, x.IdCodigo, x.IdTipo });

                // Puesto -> Ocupaciones (Restrict para evitar múltiples rutas de cascada en SQL Server)
                e.HasMany(x => x.Ocupaciones)
                 .WithOne(o => o.Puesto)
                 .HasForeignKey(o => o.IdPuesto)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== CIUDAD =====================
            modelBuilder.Entity<Ciudad>(e =>
            {
                e.ToTable("Ciudades");
                e.HasKey(x => x.Id);
                e.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
                e.HasIndex(x => x.Nombre).IsUnique();

                e.HasData(
                    new Ciudad { Id = 1, Nombre = "Sonsón" },
                    new Ciudad { Id = 2, Nombre = "La Unión" },
                    new Ciudad { Id = 3, Nombre = "Rionegro" },
                    new Ciudad { Id = 4, Nombre = "La Ceja" },
                    new Ciudad { Id = 5, Nombre = "Medellín" }
                );

                e.HasMany(x => x.ViajesOrigen)
                 .WithOne(v => v.CiudadOrigen)
                 .HasForeignKey(v => v.CiudadOrigenId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(x => x.ViajesDestino)
                 .WithOne(v => v.CiudadDestino)
                 .HasForeignKey(v => v.CiudadDestinoId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(x => x.RutasComoOrigen)
                 .WithOne(r => r.CiudadOrigen)
                 .HasForeignKey(r => r.CiudadOrigenId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(x => x.RutasComoDestino)
                 .WithOne(r => r.CiudadDestino)
                 .HasForeignKey(r => r.CiudadDestinoId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== RUTA =====================
            modelBuilder.Entity<Ruta>(e =>
            {
                e.ToTable("Rutas");
                e.HasKey(x => x.Id);

                e.Property(x => x.PrecioBase).IsRequired().HasPrecision(18, 2);
                e.Property(x => x.DistanciaKm).IsRequired();
                e.Property(x => x.TiempoEstimado).IsRequired().HasMaxLength(50);

                // Una sola ruta por par origen-destino
                e.HasIndex(x => new { x.CiudadOrigenId, x.CiudadDestinoId }).IsUnique();
            });

            // ===================== HORA =====================
            modelBuilder.Entity<Hora>(e =>
            {
                e.ToTable("Horas");
                e.HasKey(x => x.Id);
                e.Property(x => x.ValorHora).IsRequired().HasMaxLength(20);
                e.HasIndex(x => x.ValorHora).IsUnique();

                e.HasMany(x => x.Viajes)
                 .WithOne(v => v.Hora)
                 .HasForeignKey(v => v.IdHora)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== VIAJE =====================
            modelBuilder.Entity<Viaje>(e =>
            {
                e.ToTable("Viajes");
                e.HasKey(x => x.Id);

                e.Property(x => x.FechaSalida).IsRequired();
                e.Property(x => x.Precio).IsRequired().HasPrecision(18, 2);

                // Búsquedas típicas: origen + destino + fecha
                e.HasIndex(x => new { x.CiudadOrigenId, x.CiudadDestinoId, x.FechaSalida });

                // Viaje -> Ocupaciones (al borrar el viaje se borran sus reservas)
                e.HasMany(x => x.Ocupaciones)
                 .WithOne(o => o.Viaje)
                 .HasForeignKey(o => o.IdViaje)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================== PASAJERO =====================
            modelBuilder.Entity<Pasajero>(e =>
            {
                e.ToTable("Pasajeros");
                e.HasKey(x => x.Id);

                e.Property(x => x.NombreCompleto).IsRequired().HasMaxLength(200);
                e.Property(x => x.Celular).IsRequired().HasMaxLength(20);
                e.Property(x => x.Direccion).HasMaxLength(250); // opcional

                e.HasMany(x => x.Ocupaciones)
                 .WithOne(o => o.Pasajero)
                 .HasForeignKey(o => o.IdPasajero)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // ===================== OCUPACION =====================
            modelBuilder.Entity<Ocupacion>(e =>
            {
                e.ToTable("Ocupaciones");
                e.HasKey(x => x.Id);
                e.Property(x => x.Ocupado).IsRequired();

                // Un puesto solo se puede ocupar una vez por viaje
                e.HasIndex(x => new { x.IdViaje, x.IdPuesto }).IsUnique();
            });

            // ===================== FALLA =====================
            modelBuilder.Entity<Falla>(e =>
            {
                e.ToTable("Fallas");
                e.HasKey(x => x.Id);

                e.Property(x => x.Descripcion).IsRequired().HasMaxLength(500);
                e.Property(x => x.Severidad).IsRequired().HasMaxLength(30);
                e.Property(x => x.Fecha).IsRequired();
                e.Property(x => x.EstaResuelta).IsRequired();
            });
        }

    }
}