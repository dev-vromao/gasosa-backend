using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gasosa_backend.Models
{
    [Table("postos")]
    public class Posto
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nome")]
        public required string Nome { get; set; }

        [MaxLength(100)]
        [Column("bandeira")]
        public string? Bandeira { get; set; }

        [Required]
        [Column("latitude", TypeName = "numeric(10,8)")]
        public decimal Latitude { get; set; }

        [Required]
        [Column("longitude", TypeName = "numeric(11,8)")]
        public decimal Longitude { get; set; }

        [Required]
        [Column("usuario_id")]
        public required string UsuarioId { get; set; }

        [ForeignKey(nameof(UsuarioId))]
        public Usuario? Usuario { get; set; }
    }
}