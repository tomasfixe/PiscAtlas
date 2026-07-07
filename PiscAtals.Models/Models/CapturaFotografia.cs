using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.Models.Models
{
    public class CapturaFotografia
    {
        [Key]
        public int Id { get; set; }

        public int CapturaId { get; set; }
        public Captura? Captura { get; set; }

        [Required]
        public string Url { get; set; } = string.Empty;

        public DateTime DataAdicao { get; set; } = DateTime.Now;
    }
}