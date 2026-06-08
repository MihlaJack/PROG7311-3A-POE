using FluentAssertions;
using GLMS.API.Models.DTOs;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GLMS.IntegrationTests.Controllers
{
    public class AuthControllerTests : IClassFixture<TestProgram>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public AuthControllerTests(TestProgram factory)
        {
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange
            var loginRequest = new
            {
                Username = "admin",
                Password = "Admin123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/login", loginRequest);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<dynamic>(content, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("token");
            content.Should().Contain("admin");
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShouldReturn401()
        {
            // Arrange
            var loginRequest = new
            {
                Username = "admin",
                Password = "wrongpassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Register_WithNewUser_ShouldReturnCreated()
        {
            // Arrange
            var uniqueUsername = $"testuser_{Guid.NewGuid():N}";
            var registerRequest = new
            {
                Username = uniqueUsername,
                Email = $"{uniqueUsername}@test.com",
                Password = "Test123!",
                Role = "User",
                FirstName = "Test",
                LastName = "User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/auth/register", registerRequest);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            content.Should().Contain(uniqueUsername);
        }

        [Fact]
        public async Task Register_WithDuplicateUsername_ShouldReturn409()
        {
            // Arrange - First register
            var username = $"dupuser_{Guid.NewGuid():N}";
            var registerRequest = new
            {
                Username = username,
                Email = $"{username}@test.com",
                Password = "Test123!",
                Role = "User"
            };
            await _client.PostAsJsonAsync("api/auth/register", registerRequest);

            // Act - Try to register again with same username
            var response = await _client.PostAsJsonAsync("api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task GetCurrentUser_WithoutToken_ShouldReturn401()
        {
            // Act
            var response = await _client.GetAsync("api/auth/me");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}