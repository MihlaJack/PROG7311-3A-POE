using GLMS.API.Models;
using GLMS.API.Models.DTOs;

namespace GLMS.API.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<ApiResponse<User>> RegisterAsync(RegisterRequest request);
        Task<User?> GetUserByIdAsync(Guid id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> UserExistsAsync(string username);
        string GenerateJwtToken(User user);
    }
}