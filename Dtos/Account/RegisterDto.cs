using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    [Required]
    public string Nome { get; set; }
    [Required]
    public string CPF { get; set; }
    [Required]
    public DateTime DataNascimento { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
}