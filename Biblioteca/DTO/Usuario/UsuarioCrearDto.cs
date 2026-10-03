using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.DTO.Usuario
{
    // Registrarse
    public class UsuarioCrearDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public string Celular { get; set; } = string.Empty;
        public bool Estado { get; set; } = true;

        // Si viene vacío, se conserva el valor por defecto de la entidad ("Conductor")
        public string? Rol { get; set; }

        public string? Foto { get; set; }
    }
}
