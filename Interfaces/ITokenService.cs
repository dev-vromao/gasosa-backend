using gasosa_backend.Models;

namespace gasosa_backend.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(Usuario user);
    }
}
