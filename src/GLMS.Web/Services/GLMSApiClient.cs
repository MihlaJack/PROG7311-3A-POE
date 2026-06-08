using GLMS.Web.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace GLMS.Web.Services
{
    public class GLMSApiClient : IGLMSApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GLMSApiClient> _logger;

        public GLMSApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, ILogger<GLMSApiClient> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void AddAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWTToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // ==================== AUTH ====================
        public async Task<LoginResponse?> LoginAsync(LoginViewModel model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", model);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login API call failed");
                return null;
            }
        }

        public async Task<bool> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                var request = new
                {
                    model.Username,
                    model.Email,
                    model.Password,
                    Role = "User",
                    model.FirstName,
                    model.LastName
                };

                var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Register API call failed");
                return false;
            }
        }

        public async Task<UserSession?> GetCurrentUserAsync()
        {
            AddAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/auth/me");
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<dynamic>();
                    // Parse user data
                    return new UserSession
                    {
                        Username = user?.username ?? "",
                        Role = user?.role ?? "User"
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get current user API call failed");
                return null;
            }
        }

        // ==================== CONTRACTS ====================
        public async Task<PagedContractsViewModel?> GetContractsAsync(ContractFilterViewModel filter)
        {
            AddAuthHeader();
            try
            {
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filter.SearchTerm)) queryParams.Add($"searchTerm={Uri.EscapeDataString(filter.SearchTerm)}");
                if (!string.IsNullOrEmpty(filter.Status)) queryParams.Add($"status={filter.Status}");
                if (!string.IsNullOrEmpty(filter.Type)) queryParams.Add($"type={filter.Type}");
                queryParams.Add($"pageNumber={filter.PageNumber}");
                queryParams.Add($"pageSize={filter.PageSize}");

                var url = "api/contracts" + (queryParams.Any() ? "?" + string.Join("&", queryParams) : "");
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ContractViewModel>>();
                    if (pagedResponse != null)
                    {
                        return new PagedContractsViewModel
                        {
                            Items = pagedResponse.Items,
                            PageNumber = pagedResponse.PageNumber,
                            PageSize = pagedResponse.PageSize,
                            TotalPages = pagedResponse.TotalPages,
                            TotalCount = pagedResponse.TotalCount
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get contracts API call failed");
                return null;
            }
        }

        public async Task<ContractViewModel?> GetContractAsync(Guid id)
        {
            AddAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/contracts/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ContractViewModel>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get contract API call failed for ID {Id}", id);
                return null;
            }
        }

        public async Task<bool> CreateContractAsync(CreateContractViewModel model)
        {
            AddAuthHeader();
            try
            {
                var request = new
                {
                    model.ContractNumber,
                    model.ClientName,
                    model.ClientEmail,
                    model.ClientPhone,
                    model.StartDate,
                    model.EndDate,
                    model.ContractValue,
                    model.Currency,
                    Type = Enum.Parse<dynamic>(model.Type),
                    model.Description,
                    model.ServiceLevelAgreement
                };

                var response = await _httpClient.PostAsJsonAsync("api/contracts", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create contract API call failed");
                return false;
            }
        }

        public async Task<bool> UpdateContractStatusAsync(Guid id, UpdateContractStatusViewModel model)
        {
            AddAuthHeader();
            try
            {
                var request = new
                {
                    Status = Enum.Parse<dynamic>(model.Status),
                    model.Reason
                };

                var response = await _httpClient.PatchAsJsonAsync($"api/contracts/{id}/status", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update contract status API call failed for ID {Id}", id);
                return false;
            }
        }

        public async Task<bool> ActivateContractAsync(Guid id)
        {
            AddAuthHeader();
            try
            {
                var response = await _httpClient.PostAsync($"api/contracts/{id}/activate", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Activate contract API call failed for ID {Id}", id);
                return false;
            }
        }

        public async Task<bool> ExpireContractAsync(Guid id)
        {
            AddAuthHeader();
            try
            {
                var response = await _httpClient.PostAsync($"api/contracts/{id}/expire", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Expire contract API call failed for ID {Id}", id);
                return false;
            }
        }

        public async Task<bool> DeleteContractAsync(Guid id)
        {
            AddAuthHeader();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/contracts/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete contract API call failed for ID {Id}", id);
                return false;
            }
        }

        public async Task<List<ContractViewModel>?> GetExpiringContractsAsync(int days = 30)
        {
            AddAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/contracts/expiring?days={days}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ContractViewModel>>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get expiring contracts API call failed");
                return null;
            }
        }

        // ==================== SERVICE REQUESTS ====================
        public async Task<PagedServiceRequestsViewModel?> GetServiceRequestsAsync(int page = 1, int pageSize = 10)
        {
            AddAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/servicerequests?pageNumber={page}&pageSize={pageSize}");
                if (response.IsSuccessStatusCode)
                {
                    var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<ServiceRequestViewModel>>();
                    if (pagedResponse != null)
                    {
                        return new PagedServiceRequestsViewModel
                        {
                            Items = pagedResponse.Items,
                            PageNumber = pagedResponse.PageNumber,
                            PageSize = pagedResponse.PageSize,
                            TotalPages = pagedResponse.TotalPages,
                            TotalCount = pagedResponse.TotalCount
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get service requests API call failed");
                return null;
            }
        }

        public async Task<ServiceRequestViewModel?> GetServiceRequestAsync(Guid id)
        {
            AddAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync($"api/servicerequests/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ServiceRequestViewModel>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get service request API call failed for ID {Id}", id);
                return null;
            }
        }

        public async Task<bool> CreateServiceRequestAsync(CreateServiceRequestViewModel model)
        {
            AddAuthHeader();
            try
            {
                var request = new
                {
                    model.ContractId,
                    Type = Enum.Parse<dynamic>(model.Type),
                    model.Description,
                    model.EstimatedCost,
                    model.ScheduledDate,
                    model.PickupLocation,
                    model.DeliveryLocation
                };

                var response = await _httpClient.PostAsJsonAsync("api/servicerequests", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create service request API call failed");
                return false;
            }
        }

        public async Task<bool> UpdateServiceRequestStatusAsync(Guid id, string status, decimal? actualCost = null)
        {
            AddAuthHeader();
            try
            {
                var request = new
                {
                    Status = Enum.Parse<dynamic>(status),
                    ActualCost = actualCost,
                    CompletionDate = status == "Completed" ? DateTime.UtcNow : (DateTime?)null
                };

                var response = await _httpClient.PatchAsJsonAsync($"api/servicerequests/{id}/status", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update service request status API call failed for ID {Id}", id);
                return false;
            }
        }

        public async Task<bool> DeleteServiceRequestAsync(Guid id)
        {
            AddAuthHeader();
            try
            {
                var response = await _httpClient.DeleteAsync($"api/servicerequests/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete service request API call failed for ID {Id}", id);
                return false;
            }
        }

        public async Task<List<ServiceRequestViewModel>?> GetPendingServiceRequestsAsync()
        {
            AddAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/servicerequests/pending");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ServiceRequestViewModel>>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get pending service requests API call failed");
                return null;
            }
        }

        // ==================== CURRENCY ====================
        public async Task<CurrencyConversionResult?> ConvertCurrencyAsync(decimal amount, string from, string to)
        {
            try
            {
                var request = new { Amount = amount, FromCurrency = from, ToCurrency = to };
                var response = await _httpClient.PostAsJsonAsync("api/currency/convert", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CurrencyConversionResult>();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Currency conversion API call failed");
                return null;
            }
        }
    }
}