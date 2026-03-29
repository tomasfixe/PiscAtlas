using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiscAtlas.Models.Models
{
    public enum EstadoDenuncia { Pendente, Analisada_Valida, Analisada_Fraude }

    public class Denuncia
    {
        [Key]
        public int DenunciaId { get; set; }

        [Required]
        public int CapturaId { get; set; }
        public virtual Captura Captura { get; set; }

        [Required]
        public string DenuncianteEmail { get; set; }

        [Required]
        public string Motivo { get; set; }

        public EstadoDenuncia Estado { get; set; } = EstadoDenuncia.Pendente;
        public string DecisaoAdmin { get; set; }
        public DateTime? DataDecisao { get; set; }
    }
}
