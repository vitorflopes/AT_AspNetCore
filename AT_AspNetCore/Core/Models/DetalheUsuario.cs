using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Models
{
    public class DetalheUsuario : IdentityUser
    {
        [Required]
        [Display(Name = "Nome de Usuário")]
        public override string UserName { get; set; }

        [Required]
        [Display(Name = "Senha")]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Email")]
        public override string Email { get; set; }

        [Display(Name = "Nome")]
        [Required]
        public string PrimeiroNome { get; set; }

        [Display(Name = "Sobrenome")]
        [Required]
        public string SegundoNome { get; set; }

        [Display(Name = "Data de Nascimento")]
        [Required, DataType(DataType.Date)]
        public DateTime Nascimento { get; set; }

        [Display(Name = "Foto")]
        public string PathImage { get; set; }

        [NotMapped]
        public IFormFile? Image { get; set; }

        [Required]
        [Display(Name = "Assuntos de Interesse")]
        public string AssuntosDeInteresse { get; set; }

        public DetalheUsuario IdentityUser { get; set; }

        public ICollection<Amizade> AmizadesSolicitadas { get; set; }
        public ICollection<Amizade> AmizadesRecebidas { get; set; }
    }
}
