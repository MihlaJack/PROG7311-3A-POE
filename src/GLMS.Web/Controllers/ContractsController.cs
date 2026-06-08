using GLMS.Web.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    [Authorize(Roles = "Admin,Manager,User")]
    public class ContractsController : Controller
    {
        private readonly IGLMSApiClient _apiClient;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(IGLMSApiClient apiClient, ILogger<ContractsController> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<IActionResult> Index(ContractFilterViewModel? filter = null)
        {
            filter ??= new ContractFilterViewModel();
            var contracts = await _apiClient.GetContractsAsync(filter);
            return View(contracts ?? new PagedContractsViewModel());
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var contract = await _apiClient.GetContractAsync(id);
            if (contract == null)
            {
                TempData["Error"] = "Contract not found.";
                return RedirectToAction("Index");
            }
            return View(contract);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Create()
        {
            return View(new CreateContractViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContractViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var success = await _apiClient.CreateContractAsync(model);
            if (success)
            {
                TempData["Success"] = "Contract created successfully!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Failed to create contract. Please try again.");
            return View(model);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(Guid id)
        {
            var success = await _apiClient.ActivateContractAsync(id);
            if (success)
            {
                TempData["Success"] = "Contract activated successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to activate contract.";
            }
            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Expire(Guid id)
        {
            var success = await _apiClient.ExpireContractAsync(id);
            if (success)
            {
                TempData["Success"] = "Contract expired successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to expire contract.";
            }
            return RedirectToAction("Details", new { id });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _apiClient.DeleteContractAsync(id);
            if (success)
            {
                TempData["Success"] = "Contract deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to delete contract. Only draft contracts with no service requests can be deleted.";
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Expiring()
        {
            var contracts = await _apiClient.GetExpiringContractsAsync(30);
            return View(contracts ?? new List<ContractViewModel>());
        }
    }
}