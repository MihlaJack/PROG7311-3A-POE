using GLMS.API.Models.DTOs;
using GLMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.API.Controllers
{
    /// <summary>
    /// Currency conversion and exchange rate operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(ICurrencyService currencyService, ILogger<CurrencyController> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        /// <summary>
        /// Convert currency from one type to another
        /// </summary>
        /// <param name="request">Conversion details (amount, from, to)</param>
        /// <returns>Converted amount with exchange rate</returns>
        [HttpPost("convert")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CurrencyConversionResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CurrencyConversionResultDto>> ConvertCurrency([FromBody] CurrencyConversionDto request)
        {
            try
            {
                if (request.Amount <= 0)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Amount must be greater than zero"));
                }

                var result = await _currencyService.ConvertCurrencyAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Currency conversion error");
                return BadRequest(ApiResponse<object>.ErrorResponse("Currency conversion failed", new List<string> { ex.Message }));
            }
        }

        /// <summary>
        /// Get latest exchange rates for a base currency
        /// </summary>
        /// <param name="baseCurrency">Base currency code (default: USD)</param>
        /// <returns>Exchange rates dictionary</returns>
        [HttpGet("rates")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ExchangeRateDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExchangeRateDto>> GetRates([FromQuery] string baseCurrency = "USD")
        {
            var rates = await _currencyService.GetLatestRatesAsync(baseCurrency);
            return Ok(rates);
        }

        /// <summary>
        /// Get list of supported currencies
        /// </summary>
        /// <returns>Dictionary of currency codes and rates</returns>
        [HttpGet("currencies")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Dictionary<string, decimal>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Dictionary<string, decimal>>> GetSupportedCurrencies()
        {
            var currencies = await _currencyService.GetSupportedCurrenciesAsync();
            return Ok(currencies);
        }
    }
}