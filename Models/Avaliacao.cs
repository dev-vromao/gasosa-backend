using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gasosa_backend.Models
{
    public class Avaliacao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }


        [Required]
        public int PostoId { get; set; }

        [ForeignKey("PostoId")]
        public Posto? Posto { get; set; }

        [Range(1, 5)]
        public int NotaGeral { get; set; }

        [Range(1, 5)]
        public int? NotaPrecos { get; set; }

        [Range(1, 5)]
        public int? NotaAtendimento { get; set; }

        public bool TemLojaConveniencia { get; set; }
        public bool TemCalibrador { get; set; }
        public bool TemLavaRapido { get; set; }
        public bool TemTrocaOleo { get; set; }
        public bool TemAreaDescanso { get; set; }
        public bool TemCarregadorEletrico { get; set; }

        [MaxLength(500)]
        public string? Comentario { get; set; }

        public DateTime DataAvaliacao { get; set; } = DateTime.UtcNow;
    }
}
