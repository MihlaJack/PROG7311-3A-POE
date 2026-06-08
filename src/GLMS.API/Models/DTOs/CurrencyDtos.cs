namespace GLMS.API.Models.DTOs
{
    public class CurrencyConversionDto
    {
        public decimal Amount { get; set; }
        public string FromCurrency { get; set; } = "USD";
        public string ToCurrency { get; set; } = "EUR";
    }

    public class CurrencyConversionResultDto
    {
        public decimal OriginalAmount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
        public DateTime ConversionDate { get; set; } = DateTime.UtcNow;
    }

    public class ExchangeRateDto
    {
        public string BaseCurrency { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}