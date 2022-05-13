using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.Models
{
    public class Post
    {
        [Key]
        public Guid PostId { get; set; }
        public string NomeUsuario { get; set; }
        public string Mensagem { get; set; }
        [Display(Name = "Foto")]
        public string PathImagePost { get; set; }

        public string UsuarioId { get; set; }

        [NotMapped]
        public IFormFile ImagePost { get; set; }

        [DataType(DataType.Date)]
        public DateTime dataPublicação { get; set; }
    }
}
