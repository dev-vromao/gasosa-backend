using System.ComponentModel.DataAnnotations;

namespace gasosa_backend.Dtos.Avaliacao
{
    public class CreateAvaliacaoDto
    {
        [Required]
        public int PostoId { get; set; }

        // Nota de qualidade geral do posto (obrigatória)
        [Range(1, 5)]
        public int NotaGeral { get; set; }

        // Notas opcionais para aspectos específicos
        [Range(1, 5)]
        public int? NotaPrecos { get; set; }

        [Range(1, 5)]
        public int? NotaAtendimento { get; set; }

        // Serviços / estrutura
        public bool TemLojaConveniencia { get; set; }
        public bool TemCalibrador { get; set; }
        public bool TemLavaRapido { get; set; }
        public bool TemTrocaOleo { get; set; }
        public bool TemAreaDescanso { get; set; }
        public bool TemCarregadorEletrico { get; set; }

        [MaxLength(500)]
        public string? Comentario { get; set; }
    }
}
