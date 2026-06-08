using GLMS.Web.Models;

namespace GLMS.Web.Services
{
    public interface IGLMSApiClient
    {
        // Auth
        Task<LoginResponse?> LoginAsync(LoginViewModel model);
        Task<bool> RegisterAsync(RegisterViewModel model);
        Task<UserSession?> GetCurrentUserAsync();

        // Contracts
        Task<PagedContractsViewModel?> GetContractsAsync(ContractFilterViewModel filter);
        Task<ContractViewModel?> GetContractAsync(Guid id);
        Task<bool> CreateContractAsync(CreateContractViewModel model);
        Task<bool> UpdateContractStatusAsync(Guid id, UpdateContractStatusViewModel model);
        Task<bool> ActivateContractAsync(Guid id);
        Task<bool> ExpireContractAsync(Guid id);
        Task<bool> DeleteContractAsync(Guid id);
        Task<List<ContractViewModel>?> GetExpiringContractsAsync(int days = 30);

        // Service Requests
        Task<PagedServiceRequestsViewModel?> GetServiceRequestsAsync(int page = 1, int pageSize = 10);
        Task<ServiceRequestViewModel?> GetServiceRequestAsync(Guid id);
        Task<bool> CreateServiceRequestAsync(CreateServiceRequestViewModel model);
        Task<bool> UpdateServiceRequestStatusAsync(Guid id, string status, decimal? actualCost = null);
        Task<bool> DeleteServiceRequestAsync(Guid id);
        Task<List<ServiceRequestViewModel>?> GetPendingServiceRequestsAsync();

        // Currency
        Task<CurrencyConversionResult?> ConvertCurrencyAsync(decimal amount, string from, string to);
    }

    public class CurrencyConversionResult
    {
        public decimal OriginalAmount { get; set; }
        public decimal ConvertedAmount { get; set; }
        public string FromCurrency { get; set; } = string.Empty;
        public string ToCurrency { get; set; } = string.Empty;
        public decimal ExchangeRate { get; set; }
    }
}