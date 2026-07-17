using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.Models.Models
{
    public class Seguidor
    {
        // O ID de quem está a clicar no botão "Seguir"
        [Required]
        public string SeguidorId { get; set; } = string.Empty;
        public virtual Utilizador? UtilizadorSeguidor { get; set; }

        // O ID do perfil que está a ser seguido
        [Required]
        public string SeguidoId { get; set; } = string.Empty;
        public virtual Utilizador? UtilizadorSeguido { get; set; }

        public DateTime DataSeguimento { get; set; } = DateTime.Now;

        // Pedido de seguimento pendente (para perfis privados)
        public bool Pendente { get; set; } = false;
    }
}