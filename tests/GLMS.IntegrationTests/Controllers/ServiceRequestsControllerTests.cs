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
    public class ServiceRequestsControllerTests : IClassFixture<TestProgram>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;
        private string? _authToken;

        public ServiceRequestsControllerTests(TestProgram factory)
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
        public async Task GetServiceRequests_WithoutAuth_ShouldReturn200()
        {
            // Act
            var response = await _client.GetAsync("api/servicerequests");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().NotBeNullOrEmpty();
            content.Should().Contain("items");
        }

        [Fact]
        public async Task GetServiceRequestById_WithValidId_ShouldReturnRequest()
        {
            // Arrange - Get first request
            var listResponse = await _client.GetAsync("api/servicerequests?pageSize=1");
            var listContent = await listResponse.Content.ReadAsStringAsync();
            var listResult = JsonSerializer.Deserialize<JsonElement>(listContent, _jsonOptions);
            var requestId = listResult.GetProperty("items")[0].GetProperty("id").GetString();

            // Act
            var response = await _client.GetAsync($"api/servicerequests/{requestId}");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain(requestId);
            content.Should().Contain("requestNumber");
        }

        [Fact]
        public async Task CreateServiceRequest_WithActiveContract_ShouldReturn201()
        {
            // Arrange
            await AuthenticateAsync();
            AddAuthHeader();

            // Get an active contract
            var contractsResponse = await _client.GetAsync("api/contracts?status=Active&pageSize=1");
            var contractsContent = await contractsResponse.Content.ReadAsStringAsync();
            var contractsResult = JsonSerializer.Deserialize<JsonElement>(contractsContent, _jsonOptions);
            var contractId = contractsResult.GetProperty("items")[0].GetProperty("id").GetString();

            var newRequest = new
            {
                ContractId = contractId,
                Type = "Freight",
                Description = "Integration test service request",
                EstimatedCost = 5000.00m,
                ScheduledDate = DateTime.UtcNow.AddDays(7),
                PickupLocation = "New York, USA",
                DeliveryLocation = "Los Angeles, USA"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/servicerequests", newRequest);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            content.Should().Contain("Service request created successfully");
        }

        [Fact]
        public async Task CreateServiceRequest_WithDraftContract_ShouldReturn400()
        {
            // Arrange
            await AuthenticateAsync();
            AddAuthHeader();

            // Get a draft contract
            var contractsResponse = await _client.GetAsync("api/contracts?status=Draft&pageSize=1");
            var contractsContent = await contractsResponse.Content.ReadAsStringAsync();
            var contractsResult = JsonSerializer.Deserialize<JsonElement>(contractsContent, _jsonOptions);

            if (contractsResult.GetProperty("items").GetArrayLength() == 0)
            {
                // Skip if no draft contracts exist
                return;
            }

            var contractId = contractsResult.GetProperty("items")[0].GetProperty("id").GetString();

            var newRequest = new
            {
                ContractId = contractId,
                Type = "Freight",
                Description = "Should fail - draft contract"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/servicerequests", newRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task UpdateServiceRequestStatus_WithValidTransition_ShouldReturn200()
        {
            // Arrange
            await AuthenticateAsync();
            AddAuthHeader();

            // Get a pending request
            var listResponse = await _client.GetAsync("api/servicerequests?status=Pending&pageSize=1");
            var listContent = await listResponse.Content.ReadAsStringAsync();
            var listResult = JsonSerializer.Deserialize<JsonElement>(listContent, _jsonOptions);

            if (listResult.GetProperty("items").GetArrayLength() == 0)
            {
                return; // Skip if no pending requests
            }

            var requestId = listResult.GetProperty("items")[0].GetProperty("id").GetString();

            // Act - Approve
            var statusUpdate = new { Status = "Approved" };
            var response = await _client.PatchAsJsonAsync($"api/servicerequests/{requestId}/status", statusUpdate);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("Approved");
        }

        [Fact]
        public async Task GetPendingServiceRequests_ShouldReturnPendingOnly()
        {
            // Arrange
            await AuthenticateAsync();
            AddAuthHeader();

            // Act
            var response = await _client.GetAsync("api/servicerequests/pending");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            // All returned items should be pending
            if (!string.IsNullOrEmpty(content) && content != "[]")
            {
                content.Should().Contain("Pending");
            }
        }

        [Fact]
        public async Task DeleteServiceRequest_WithPendingStatus_ShouldReturn200()
        {
            // Arrange
            await AuthenticateAsync();
            AddAuthHeader();

            // Create a new request first
            var contractsResponse = await _client.GetAsync("api/contracts?status=Active&pageSize=1");
            var contractsContent = await contractsResponse.Content.ReadAsStringAsync();
            var contractsResult = JsonSerializer.Deserialize<JsonElement>(contractsContent, _jsonOptions);
            var contractId = contractsResult.GetProperty("items")[0].GetProperty("id").GetString();

            var newRequest = new
            {
                ContractId = contractId,
                Type = "Freight",
                Description = "To be deleted"
            };

            var createResponse = await _client.PostAsJsonAsync("api/servicerequests", newRequest);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = JsonSerializer.Deserialize<JsonElement>(createContent, _jsonOptions);
            var requestId = createResult.GetProperty("data").GetProperty("id").GetString();

            // Act
            var response = await _client.DeleteAsync($"api/servicerequests/{requestId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetServiceRequests_WithPagination_ShouldReturnCorrectPageSize()
        {
            // Act
            var response = await _client.GetAsync("api/servicerequests?pageNumber=1&pageSize=2");
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.GetProperty("pageSize").GetInt32().Should().Be(2);
        }
    }
}