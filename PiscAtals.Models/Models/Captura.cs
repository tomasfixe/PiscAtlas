using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiscAtlas.Models.Models
{
    public class Captura
    {
        [Key]
        public int CapturaId { get; set; }

        [Required(ErrorMessage = "Tem de selecionar uma espécie.")]
        public int EspecieId { get; set; }
        public Especie? Especie { get; set; }

        [Required]
        public int PesqueiroId { get; set; }
        public virtual Pesqueiro? Pesqueiro { get; set; }

        [Required]
        public string FotografiaUrl { get; set; }

        public double? Peso { get; set; } // Opcional
        public double? Tamanho { get; set; } // Opcional

        public bool PossuiProvasVisuais { get; set; }
        public bool AprovadaPeloAdmin { get; set; } = false; // Admin aprova se houver peso/tamanho

        public bool FraudeConfirmada { get; set; } = false;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Notas { get; set; }
        public string? Descricao { get; set; }

        [Required]
        public string UtilizadorId { get; set; }
        public virtual Utilizador? Utilizador { get; set; }

        public DateTime DataCaptura { get; set; } = DateTime.Now;

        // Relacionamento com Denúncias
        public virtual ICollection<Denuncia>? Denuncias { get; set; }
        // Isto permite que uma captura tenha N fotografias associadas
        public ICollection<CapturaFotografia> Fotografias { get; set; } = new List<CapturaFotografia>();

        public virtual ICollection<Interacao> Interacoes { get; set; } = new List<Interacao>();
    }
}
