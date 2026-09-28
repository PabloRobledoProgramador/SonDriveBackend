using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.DTO.Usuario
{
    public class UsuarioActualizarDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public string? Rol { get; set; }
        public string? Foto { get; set; }
    }
}
