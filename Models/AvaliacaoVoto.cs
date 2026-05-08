using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gasosa_backend.Models
{
    public class AvaliacaoVoto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AvaliacaoId { get; set; }

        [ForeignKey("AvaliacaoId")]
        public Avaliacao? Avaliacao { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        /// <summary>
        /// true = like, false = dislike
        /// </summary>
        public bool IsLike { get; set; }

        public DateTime DataVoto { get; set; } = DateTime.UtcNow;
    }
}
