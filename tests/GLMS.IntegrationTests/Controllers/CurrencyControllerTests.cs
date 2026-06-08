using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace GLMS.IntegrationTests.Controllers
{
    public class CurrencyControllerTests : IClassFixture<TestProgram>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public CurrencyControllerTests(TestProgram factory)
        {
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Fact]
        public async Task ConvertCurrency_WithValidRequest_ShouldReturnResult()
        {
            // Arrange
            var request = new
            {
                Amount = 100.00m,
                FromCurrency = "USD",
                ToCurrency = "EUR"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/currency/convert", request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Contain("convertedAmount");
            content.Should().Contain("exchangeRate");
        }

        [Fact]
        public async Task GetLatestRates_ShouldReturnRates()
        {
            // Act
            var response = await _client.GetAsync("api/currency/rates?baseCurrency=USD");
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content, _jsonOptions);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            result.GetProperty("baseCurrency").GetString().Should().Be("USD");
            result.GetProperty("rates").EnumerateObject().Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetSupportedCurrencies_ShouldReturnCurrencyList()
        {
            // Act
            var response = await _client.GetAsync("api/currency/currencies");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ConvertCurrency_WithZeroAmount_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new
            {
                Amount = 0m,
                FromCurrency = "USD",
                ToCurrency = "EUR"
            };

            // Act
            var response = await _client.PostAsJsonAsync("api/currency/convert", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}