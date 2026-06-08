using GLMS.API.Models.DTOs;
using System.Text.Json;

namespace GLMS.API.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ICacheService _cache;
        private readonly ILogger<CurrencyService> _logger;
        private readonly IConfiguration _configuration;

        public CurrencyService(HttpClient httpClient, ICacheService cache, ILogger<CurrencyService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<CurrencyConversionResultDto> ConvertCurrencyAsync(CurrencyConversionDto request)
        {
            var cacheKey = $"conversion_{request.FromCurrency}_{request.ToCurrency}_{request.Amount}";
            var cached = await _cache.GetAsync<CurrencyConversionResultDto>(cacheKey);
            if (cached != null) return cached;

            try
            {
                var apiKey = _configuration["CurrencyApi:Key"];
                var url = $"/convert?from={request.FromCurrency}&to={request.ToCurrency}&amount={request.Amount}&access_key={apiKey}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);

                var rate = result.GetProperty("info").GetProperty("rate").GetDecimal();
                var convertedAmount = result.GetProperty("result").GetDecimal();

                var conversionResult = new CurrencyConversionResultDto
                {
                    OriginalAmount = request.Amount,
                    ConvertedAmount = convertedAmount,
                    FromCurrency = request.FromCurrency,
                    ToCurrency = request.ToCurrency,
                    ExchangeRate = rate
                };

                await _cache.SetAsync(cacheKey, conversionResult, TimeSpan.FromMinutes(30));
                return conversionResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Currency conversion failed for {Amount} {From} to {To}", 
                    request.Amount, request.FromCurrency, request.ToCurrency);

                // Fallback to cached rates
                var rates = await GetLatestRatesAsync(request.FromCurrency);
                if (rates.Rates.ContainsKey(request.ToCurrency))
                {
                    var rate = rates.Rates[request.ToCurrency];
                    return new CurrencyConversionResultDto
                    {
                        OriginalAmount = request.Amount,
                        ConvertedAmount = request.Amount * rate,
                        FromCurrency = request.FromCurrency,
                        ToCurrency = request.ToCurrency,
                        ExchangeRate = rate
                    };
                }

                throw new InvalidOperationException($"Currency conversion failed and no fallback available for {request.FromCurrency} to {request.ToCurrency}");
            }
        }

        public async Task<ExchangeRateDto> GetLatestRatesAsync(string baseCurrency = "USD")
        {
            var cacheKey = $"rates_{baseCurrency}";
            var cached = await _cache.GetAsync<ExchangeRateDto>(cacheKey);
            if (cached != null) return cached;

            try
            {
                var apiKey = _configuration["CurrencyApi:Key"];
                var url = $"/latest?base={baseCurrency}&access_key={apiKey}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(content);

                var rates = new Dictionary<string, decimal>();
                var ratesElement = result.GetProperty("rates");
                foreach (var property in ratesElement.EnumerateObject())
                {
                    rates[property.Name] = property.Value.GetDecimal();
                }

                var exchangeRates = new ExchangeRateDto
                {
                    BaseCurrency = baseCurrency,
                    Date = DateTime.UtcNow,
                    Rates = rates
                };

                await _cache.SetAsync(cacheKey, exchangeRates, TimeSpan.FromHours(1));
                return exchangeRates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get latest rates for {BaseCurrency}", baseCurrency);

                // Return embedded fallback rates
                return new ExchangeRateDto
                {
                    BaseCurrency = baseCurrency,
                    Date = DateTime.UtcNow,
                    Rates = new Dictionary<string, decimal>
                    {
                        ["USD"] = 1.0m,
                        ["EUR"] = 0.85m,
                        ["GBP"] = 0.73m,
                        ["JPY"] = 110.0m,
                        ["CAD"] = 1.25m,
                        ["AUD"] = 1.35m,
                        ["CHF"] = 0.92m,
                        ["CNY"] = 6.45m
                    }
                };
            }
        }

        public async Task<Dictionary<string, decimal>> GetSupportedCurrenciesAsync()
        {
            var rates = await GetLatestRatesAsync("USD");
            return rates.Rates;
        }
    }
}