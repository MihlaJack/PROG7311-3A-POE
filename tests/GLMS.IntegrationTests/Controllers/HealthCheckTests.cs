using FluentAssertions;
using System.Net;
using Xunit;

namespace GLMS.IntegrationTests.Controllers
{
    public class HealthCheckTests : IClassFixture<TestProgram>
    {
        private readonly HttpClient _client;

        public HealthCheckTests(TestProgram factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task HealthCheck_ShouldReturn200()
        {
            // Act
            var response = await _client.GetAsync("health");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task SwaggerEndpoint_ShouldReturn200()
        {
            // Act
            var response = await _client.GetAsync("swagger/v1/swagger.json");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}