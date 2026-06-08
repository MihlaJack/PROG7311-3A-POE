using FluentAssertions;
using GLMS.API.Models;
using GLMS.API.Models.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GLMS.IntegrationTests.Controllers
{
    public class ContractsControllerTests : IClassFixture<TestProgram>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;
        private string? _authToken;

        public ContractsControllerTests(TestProgram factory)
        {
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        private async Task AuthenticateAsync()
        {
            if (_authToken != null) return;

            var loginRequest = new { Username = "admin", Password = "Admin123!" };
            var response = await _client.PostAsJsonAsync("api/auth/login", loginRequest);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);
            _authToken = result.GetProperty("token").GetString();
        }

        private void AddAuthHeader()
        {
            if (_authToken != null)
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        }

        [Fact]
        public async Task GetContracts_WithoutAuth_ShouldReturn200()
        {
            // Act
            var response = await _client.GetAsync("api/contracts");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("items");
        }

        [Fact]
        public async Task GetContracts_WithFilter_ShouldReturnFilteredResults()
        {
            // Act
            var response = await _client.GetAsync("api/contracts?status=Active&pageSize=5");
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.GetProperty("items").GetArrayLength().Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetContractById_WithValidId_ShouldReturnContract()
        {
            // Arrange - Get first contract
            var listResponse = await _client.GetAsync("api/contracts?pageSize=1");
            var listContent = await listResponse.Content.ReadAsStringAsync();
            var listResult = JsonSerializer.Deserialize<JsonElement>(listContent, _jsonOptions);
            var contractId = listResult.GetProperty("items")[0].GetProperty("id").GetString();

            // Act
            var response = await _client.GetAsync($"api/contracts/{contractId}");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain(contractId);
            content.Should().Contain("contractNumber");
        }

        [Fact]
        public async Task GetContractById_WithInvalidId_ShouldReturn404()
        {
            // Act
            var response = await _client.GetAsync($"api/contracts/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateContract_WithAuth_ShouldReturn201()
        {
            // Arrange
            await AuthenticateAsync();
            AddAuthHeader();

            var newContract = new
            {
                ContractNumber = $"CNT-TEST-{Guid.NewGuid():N}",
                ClientName = "Test Client Corp",
                ClientEmail = "test@client.com",
                ClientPhone = "+1-555-9999",
                StartDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddMonths(12),
                ContractValue = 50000.00m,
                Currency = "USD",
                Type = "Freight",
                Description = "Integration test contract",
                ServiceLevelAgreement = "99% uptime guarantee"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/contracts", newContract);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            content.Should().Contain(newContract.ContractNumber);
            content.Should().Contain("Contract created successfully");
        }

        [Fact]
        public async Task CreateContract_WithoutAuth_ShouldReturn401()
        {
            // Arrange
            var newContract = new
            {
                ContractNumber = $"CNT-TEST-{Guid.NewGuid():N}",
                ClientName = "Test Client",
                ClientEmail = "test@test.com",
                ClientPhone = "+1-555-0000",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                ContractValue = 10000.00m,
                Currency = "USD",
                Type = "Freight"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/contracts", newContract);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateContract_WithDuplicateNumber_ShouldReturn409()
        {
            // Arrange
            await AuthenticateAsync();
            AddAuthHeader();

            var contractNumber = $"CNT-DUP-{Guid.NewGuid():N}";
            var newContract = new
            {
                ContractNumber = contractNumber,
                ClientName = "Test Client",
                ClientEmail = "test@test.com",
                ClientPhone = "+1-555-0000",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                ContractValue = 10000.00m,
                Currency = "USD",
                Type = "Freight"
            };

            // Create first
            await _client.PostAsJsonAsync("api/contracts", newContract);

            // Act - Try duplicate
            var response = await _client.PostAsJsonAsync("api/contracts", newContract);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task UpdateContractStatus_WithValidTransition_ShouldReturn200()
        {
            // Arrange
            await AuthenticateAsync();
            AddAuthHeader();

            // Create a draft contract
            var newContract = new
            {
                ContractNumber = $"CNT-STATUS-{Guid.NewGuid():N}",
                ClientName = "Status Test",
                ClientEmail = "status@test.com",
                ClientPhone = "+1-555-0000",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddMonths(6),
                ContractValue = 10000.00m,
                Currency = "USD",
                Type = "Freight"
            };

            var createResponse = await _client.PostAsJsonAsync("api/contracts", newContract);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<JsonElement>(createContent, _jsonOptions);
            var contractId = createResult.GetProperty("data").GetProperty("id").GetString();

            // Act - Activate
            var statusUpdate = new { Status = "Active", Reason = "Integration test activation" };
            var response = await _client.PatchAsJsonAsync($"api/contracts/{contractId}/status", statusUpdate);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Active");
        }

        [Fact]
        public async Task UpdateContractStatus_WithInvalidTransition_ShouldReturn400()
        {
            // Arrange
            await AuthenticateAsync();
            AddAuthHeader();

            // Get an active contract
            var listResponse = await _client.GetAsync("api/contracts?status=Active&pageSize=1");
            var listContent = await listResponse.Content.ReadAsStringAsync();
            var listResult = JsonSerializer.Deserialize<JsonElement>(listContent, _jsonOptions);
            var contractId = listResult.GetProperty("items")[0].GetProperty("id").GetString();

            // Act - Try invalid transition (Active -> Draft)
            var statusUpdate = new { Status = "Draft" };
            var response = await _client.PatchAsJsonAsync($"api/contracts/{contractId}/status", statusUpdate);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task DeleteContract_WithDraftStatus_ShouldReturn200()
        {
            // Arrange
            await AuthenticateAsync();
            AddAuthHeader();

            var newContract = new
            {
                ContractNumber = $"CNT-DEL-{Guid.NewGuid():N}",
                ClientName = "Delete Test",
                ClientEmail = "delete@test.com",
                ClientPhone = "+1-555-0000",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(6),
                ContractValue = 10000.00m,
                Currency = "USD",
                Type = "Freight"
            };

            var createResponse = await _client.PostAsJsonAsync("api/contracts", newContract);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<JsonElement>(createContent, _jsonOptions);
            var contractId = createResult.GetProperty("data").GetProperty("id").GetString();

            // Act
            var response = await _client.DeleteAsync($"api/contracts/{contractId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetExpiringContracts_ShouldReturnContracts()
        {
            // Arrange
            await AuthenticateAsync();
            AddAuthHeader();

            // Act
            var response = await _client.GetAsync("api/contracts/expiring?days=365");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().NotBeNull();
        }

        [Fact]
        public async Task GetContractByNumber_ShouldReturnContract()
        {
            // Arrange - Get first contract number
            var listResponse = await _client.GetAsync("api/contracts?pageSize=1");
            var listContent = await listResponse.Content.ReadAsStringAsync();
            var listResult = JsonSerializer.Deserialize<JsonElement>(listContent, _jsonOptions);
            var contractNumber = listResult.GetProperty("items")[0].GetProperty("contractNumber").GetString();

            // Act
            var response = await _client.GetAsync($"api/contracts/by-number/{contractNumber}");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain(contractNumber);
        }
    }
}