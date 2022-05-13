using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class Amizade
    {
        public string UsuarioIdA { get; set; }
        public string UsuarioIdB { get; set; }
        public DetalheUsuario UsuarioA { get; set; }
        public DetalheUsuario UsuarioB { get; set; }
    }
}
