using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager,User")]
    public class ServiceRequestsController : Controller
    {
        private readonly IGLMSApiClient _apiClient;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(IGLMSApiClient apiClient, ILogger<ServiceRequestsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var requests = await _apiClient.GetServiceRequestsAsync(page, pageSize);
            return View(requests ?? new PagedServiceRequestsViewModel());
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var request = await _apiClient.GetServiceRequestAsync(id);
            if (request == null)
            {
                TempData["Error"] = "Service request not found.";
                return RedirectToAction("Index");
            }
            return View(request);
        }

        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> Create()
        {
            // Get active contracts for dropdown
            var contracts = await _apiClient.GetContractsAsync(new ContractFilterViewModel { Status = "Active", PageSize = 100 });
            ViewBag.ActiveContracts = contracts?.Items ?? new List<ContractViewModel>();
            return View(new CreateServiceRequestViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,User")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateServiceRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var contracts = await _apiClient.GetContractsAsync(new ContractFilterViewModel { Status = "Active", PageSize = 100 });
                ViewBag.ActiveContracts = contracts?.Items ?? new List<ContractViewModel>();
                return View(model);
            }

            var success = await _apiClient.CreateServiceRequestAsync(model);
            if (success)
            {
                TempData["Success"] = "Service request created successfully!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Failed to create service request. Ensure the contract is active and not expiring soon.");
            var contractsList = await _apiClient.GetContractsAsync(new ContractFilterViewModel { Status = "Active", PageSize = 100 });
            ViewBag.ActiveContracts = contractsList?.Items ?? new List<ContractViewModel>();
            return View(model);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            var success = await _apiClient.UpdateServiceRequestStatusAsync(id, "Approved");
            if (success)
            {
                TempData["Success"] = "Service request approved!";
            }
            else
            {
                TempData["Error"] = "Failed to approve service request.";
            }
            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(Guid id, decimal? actualCost)
        {
            var success = await _apiClient.UpdateServiceRequestStatusAsync(id, "Completed", actualCost);
            if (success)
            {
                TempData["Success"] = "Service request completed!";
            }
            else
            {
                TempData["Error"] = "Failed to complete service request.";
            }
            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var success = await _apiClient.UpdateServiceRequestStatusAsync(id, "Cancelled");
            if (success)
            {
                TempData["Success"] = "Service request cancelled.";
            }
            else
            {
                TempData["Error"] = "Failed to cancel service request.";
            }
            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _apiClient.DeleteServiceRequestAsync(id);
            if (success)
            {
                TempData["Success"] = "Service request deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete service request. Only pending requests can be deleted.";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Pending()
        {
            var requests = await _apiClient.GetPendingServiceRequestsAsync();
            return View(requests ?? new List<ServiceRequestViewModel>());
        }
    }
}