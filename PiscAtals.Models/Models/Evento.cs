using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiscAtlas.Models.Models
{
    public class Evento
    {
        [Key]
        public int EventoId { get; set; }

        [Required]
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        public int EspecieAlvoId { get; set; }
        public virtual Especie EspecieAlvo { get; set; }

        public double? PesoMinimo { get; set; }
        public double? TamanhoMinimo { get; set; }
        public decimal PrecoInscricao { get; set; }

        public virtual ICollection<Inscricao> Inscricoes { get; set; }
    }
}
