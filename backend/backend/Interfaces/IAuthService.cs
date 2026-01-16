using backend.Models.Dtos;

namespace backend.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse?> LoginAsync(AuthRequest req);
    }
}
