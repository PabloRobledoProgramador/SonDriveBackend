using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Biblioteca.Modelo;


namespace Biblioteca.Conexion
{
    public class Conexion : DbContext, IConexion
    {
        // Inyecta las opciones desde Program.cs 
        public Conexion(DbContextOptions<Conexion> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
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
    }
}
