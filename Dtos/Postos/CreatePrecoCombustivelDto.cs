using System.ComponentModel.DataAnnotations;

namespace gasosa_backend.Dtos.Postos
{
    public class CreatePrecoCombustivelDto
    {
        [Required]
        public string TipoCombustivel { get; set; } = string.Empty;

        [Required]
        [Range(1, 15, ErrorMessage = "O preço deve estar entre R$ 1,00 e R$ 15,00.")]
        public decimal Preco { get; set; }
    }
}