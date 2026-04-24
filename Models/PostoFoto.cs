using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gasosa_backend.Models
{
    public class PostoFoto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostoId { get; set; }

        [Required]
        public string UrlImagem { get; set; }
    }
}