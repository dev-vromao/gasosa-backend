namespace gasosa_backend.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetCodeAsync(string toEmail, string code);
    }
}
