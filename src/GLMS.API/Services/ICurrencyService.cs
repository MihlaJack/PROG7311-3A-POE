using GLMS.API.Models.DTOs;

namespace GLMS.API.Services
{
    public interface ICurrencyService
    {
        Task<CurrencyConversionResultDto> ConvertCurrencyAsync(CurrencyConversionDto request);
        Task<ExchangeRateDto> GetLatestRatesAsync(string baseCurrency = "USD");
        Task<Dictionary<string, decimal>> GetSupportedCurrenciesAsync();
    }
}