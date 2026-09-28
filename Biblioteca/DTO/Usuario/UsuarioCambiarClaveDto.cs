using System;
using System.Collections.Generic;
using System.Text;

namespace Biblioteca.DTO.Usuario
{
    public class UsuarioCambiarClaveDto
    {
        public string ClaveActual { get; set; } = string.Empty;
        public string ClaveNueva { get; set; } = string.Empty;
    }
}
