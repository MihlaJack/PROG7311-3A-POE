using GLMS.API.Data;
using GLMS.API.Models;
using GLMS.API.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GLMS.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly GLMSDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(GLMSDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null)
            {
                _logger.LogWarning("Login failed for username: {Username} - User not found", request.Username);
                return null;
            }

            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for username: {Username} - Invalid password", request.Username);
                return null;
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(8);

            _logger.LogInformation("User {Username} logged in successfully", request.Username);

            return new LoginResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                ExpiresAt = expiresAt
            };
        }

        public async Task<ApiResponse<User>> RegisterAsync(RegisterRequest request)
        {
            if (await UserExistsAsync(request.Username))
            {
                return ApiResponse<User>.ErrorResponse(
                    "Username already exists",
                    new List<string> { $"Username '{request.Username}' is already taken." },
                    409);
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return ApiResponse<User>.ErrorResponse(
                    "Email already registered",
                    new List<string> { $"Email '{request.Email}' is already registered." },
                    409);
            }

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                Role = request.Role,
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} registered successfully", request.Username);
            return ApiResponse<User>.SuccessResponse(user, "User registered successfully");
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FirstName", user.FirstName ?? ""),
                new Claim("LastName", user.LastName ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}