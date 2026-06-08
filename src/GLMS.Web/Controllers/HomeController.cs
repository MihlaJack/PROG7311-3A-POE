using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GLMS.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGLMSApiClient _apiClient;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IGLMSApiClient apiClient, ILogger<HomeController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var dashboard = new DashboardViewModel();

            try
            {
                // Get contracts summary
                var contracts = await _apiClient.GetContractsAsync(new ContractFilterViewModel { PageSize = 5 });
                if (contracts != null)
                {
                    dashboard.RecentContracts = contracts.Items;
                    dashboard.TotalContracts = contracts.TotalCount;
                    dashboard.ActiveContracts = contracts.Items.Count(c => c.Status == "Active");
                    dashboard.ExpiringContracts = contracts.Items.Count(c => c.Status == "Active" && c.EndDate <= DateTime.Now.AddDays(30));
                }

                // Get pending service requests
                var pendingRequests = await _apiClient.GetPendingServiceRequestsAsync();
                dashboard.PendingRequests = pendingRequests ?? new List<ServiceRequestViewModel>();
                dashboard.TotalPendingRequests = dashboard.PendingRequests.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["Error"] = "Unable to load dashboard data. Please try again later.";
            }

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class DashboardViewModel
    {
        public List<ContractViewModel> RecentContracts { get; set; } = new();
        public List<ServiceRequestViewModel> PendingRequests { get; set; } = new();
        public int TotalContracts { get; set; }
        public int ActiveContracts { get; set; }
        public int ExpiringContracts { get; set; }
        public int TotalPendingRequests { get; set; }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}