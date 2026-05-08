using System.ComponentModel.DataAnnotations;

namespace gasosa_backend.Dtos.Avaliacao
{
    public class VotoDto
    {
        [Required]
        public bool IsLike { get; set; }
    }
}
