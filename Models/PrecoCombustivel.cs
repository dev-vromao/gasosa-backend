using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gasosa_backend.Models
{
    [Table("precos_combustiveis")]
    public class PrecoCombustivel
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("posto_id")]
        public int PostoId { get; set; }

        [ForeignKey(nameof(PostoId))]
        public Posto? Posto { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("tipo_combustivel")]
        public string TipoCombustivel { get; set; } = string.Empty;

        [Required]
        [Column("preco", TypeName = "numeric(5,2)")]
        public decimal Preco { get; set; }

        [Required]
        [Column("data_cadastro")]
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    }
}