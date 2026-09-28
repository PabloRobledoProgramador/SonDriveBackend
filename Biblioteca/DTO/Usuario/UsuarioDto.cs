using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.DTO.Usuario
{
    // Respuesta: nunca expone la Clave
    public class UsuarioDto
    {
        public long Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string? Foto { get; set; }
    }
}
