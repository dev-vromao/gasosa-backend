using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace gasosa_backend.Dtos.Postos
{
    public class UploadPostoFotoDto
    {
        [Required]
        public required IFormFile Foto { get; set; }
    }
}
