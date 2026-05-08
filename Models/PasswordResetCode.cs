namespace gasosa_backend.Models
{
    public class PasswordResetCode
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
