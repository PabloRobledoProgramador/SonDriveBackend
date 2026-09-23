using Biblioteca.Modelo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.Conexion
{
    public interface IConexion
    {
        DbSet<Usuario> Usuarios { get; set; }
        DbSet<Vehiculo> Vehiculos { get; set; }
        DbSet<Puesto> Puestos { get; set; }
        DbSet<Viaje> Viajes { get; set; }
        DbSet<Ocupacion> Ocupaciones { get; set; }
        DbSet<Ruta> Rutas { get; set; }
        DbSet<Ciudad> Ciudades { get; set; }
        DbSet<Hora> Horas { get; set; }
        DbSet<Falla> Fallas { get; set; }

        // Confirmar Cambios en la BD
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
