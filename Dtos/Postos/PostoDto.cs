namespace gasosa_backend.Dtos.Postos
{
    public class PostoDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string? Bandeira { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
    }
}
