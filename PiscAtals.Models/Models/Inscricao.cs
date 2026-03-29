using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiscAtlas.Models.Models
{
    public enum EstadoPagamento { Pendente, Pago, Cancelado }

    public class Inscricao
    {
        [Key]
        public int InscricaoId { get; set; }

        [Required]
        public int EventoId { get; set; }
        public virtual Evento Evento { get; set; }

        public string PescadorEmail { get; set; }
        public string PescadorNome { get; set; }

        public EstadoPagamento EstadoPagamento { get; set; } = EstadoPagamento.Pendente;
        public decimal ValorPago { get; set; }

        public int? MelhorCapturaId { get; set; }
        public double Pontuacao { get; set; }
    }
}
