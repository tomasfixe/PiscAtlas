using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiscAtals.Models.Models
{
    public enum TipoHabitat { Agua_Doce, Agua_Salgada, Ambos }

    public class Especie
    {
        [Key]
        public int EspecieId { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Nome { get; set; }

        public string NomeCientifico { get; set; }
        public string Descricao { get; set; }
        public string ImagemUrl { get; set; }

        public double? PesoRecordPt { get; set; }
        public double? TamanhoRecordPt { get; set; }

        public TipoHabitat Habitat { get; set; }

        // Relacionamento: Uma espécie pode estar em muitas capturas
        public virtual ICollection<Captura> Capturas { get; set; }
    }
}
