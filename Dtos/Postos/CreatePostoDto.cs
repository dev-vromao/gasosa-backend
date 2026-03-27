using System.ComponentModel.DataAnnotations;

public class CreatePostoDto
{
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; }

    [MaxLength(100)]
    public string? Bandeira { get; set; }

    [Required]
    public decimal Latitude { get; set; }

    [Required]
    public decimal Longitude { get; set; }
}