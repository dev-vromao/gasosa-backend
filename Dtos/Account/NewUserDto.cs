public class NewUserDto
{
    public required string Nome { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
    public bool Banido { get; set; }
}