using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.Models.Models
{
    // Define os tipos de interação possíveis
    public enum TipoInteracao
    {
        Gosto,
        Comentario
    }

    public class Interacao
    {
        [Key]
        public int InteracaoId { get; set; }

        [Required]
        public TipoInteracao Tipo { get; set; } // Guarda se é um Gosto ou Comentário

        [MaxLength(500)]
        public string? Texto { get; set; } // Fica vazio (null) nos Gostos, e preenchido nos Comentários

        [Required]
        public int CapturaId { get; set; }
        public virtual Captura? Captura { get; set; }

        [Required]
        public string UtilizadorId { get; set; } = string.Empty;
        public virtual Utilizador? Utilizador { get; set; }

        public DateTime DataInteracao { get; set; } = DateTime.Now;
    }
}