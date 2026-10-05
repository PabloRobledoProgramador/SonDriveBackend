using Biblioteca.Conexion;
using Biblioteca.Modelo;
using Biblioteca.Repositorios.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Biblioteca.Repositorios.Implementaciones
{
    public class UsuarioRepositorio : IUsuarioRepositorio
    {
        // Se asume que IConexion expone:
        //   DbSet<Usuario> Usuarios { get; }
        //   Task<int> SaveChangesAsync(CancellationToken ct = default);
        private readonly IConexion _db;

        public UsuarioRepositorio(IConexion db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Usuario>> ListarAsync(CancellationToken ct = default)
        {
            return await _db.Usuarios
                .AsNoTracking()
                .OrderBy(u => u.Apellido)
                .ThenBy(u => u.Nombre)
                .ToListAsync(ct);
        }

        public async Task<Usuario?> ObtenerPorIdAsync(long id, bool incluirVehiculos = false, CancellationToken ct = default)
        {
            IQueryable<Usuario> query = _db.Usuarios.AsNoTracking();

            if (incluirVehiculos)
                query = query.Include(u => u.Vehiculos);

            return await query.FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public async Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken ct = default)
        {
            return await _db.Usuarios
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Correo == correo, ct);
        }

        public async Task<bool> ExisteCorreoAsync(string correo, long? excluirId = null, CancellationToken ct = default)
        {
            return await _db.Usuarios.AnyAsync(
                u => u.Correo == correo && (excluirId == null || u.Id != excluirId),
                ct);
        }

        public async Task<bool> ExisteRolAsync(string rol, CancellationToken ct = default)
        {
            return await _db.Usuarios.AnyAsync(u => u.Rol == rol, ct);
        }

        public async Task<Usuario> CrearAsync(Usuario usuario, CancellationToken ct = default)
        {
            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync(ct);
            return usuario;
        }

        public async Task<Usuario> ActualizarAsync(Usuario usuario, CancellationToken ct = default)
        {
            _db.Usuarios.Update(usuario);
            await _db.SaveChangesAsync(ct);
            return usuario;
        }

        public async Task<bool> EliminarAsync(long id, CancellationToken ct = default)
        {
            var usuario = await _db.Usuarios.FindAsync(new object[] { id }, ct);
            if (usuario is null)
                return false;

            _db.Usuarios.Remove(usuario);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}