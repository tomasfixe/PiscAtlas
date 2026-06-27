using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiscAtlas.Models.Models
{
    public enum TipoPesqueiro { Rio, Lago, Barragem, Costa, Mar_Alto, Outro }

    public class Pesqueiro
    {
        [Key]
        public int PesqueiroId { get; set; }

        [Required]
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public TipoPesqueiro Tipo { get; set; }
        public string FotografiaUrl { get; set; }

        public virtual ICollection<Captura>? Capturas { get; set; }
    }
}
