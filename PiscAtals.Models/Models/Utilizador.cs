using Microsoft.AspNetCore.Identity;
using PiscAtlas.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.Models.Models
{
    public class Utilizador : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string NomeUtilizador { get; set; }

        [Required]
        [StringLength(50)]
        public string PrimeiroNome { get; set; }

        [Required]
        [StringLength(50)]
        public string UltimoNome { get; set; }

        public string FotografiaPerfilUrl { get; set; }
        public DateTime DataRegisto { get; set; } = DateTime.Now;
        public string NomeCompleto => $"{PrimeiroNome} {UltimoNome}";

        public virtual ICollection<Captura> Capturas { get; set; }
        public virtual ICollection<Inscricao> Inscricoes { get; set; }
    }
}