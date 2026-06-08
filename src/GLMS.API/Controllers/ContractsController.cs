using GLMS.API.Models;
using GLMS.API.Models.DTOs;
using GLMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GLMS.API.Controllers
{
    /// <summary>
    /// Manages logistics contracts including creation, updates, status transitions, and filtering
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly ILogger<ContractsController> _logger;

        public ContractsController(IContractService contractService, ILogger<ContractsController> logger)
        {
            _contractService = contractService;
            _logger = logger;
        }

        /// <summary>
        /// Get all contracts with advanced filtering, sorting, and pagination
        /// </summary>
        /// <param name="filter">Filter parameters including search, status, type, dates, and pagination</param>
        /// <returns>Paginated list of contracts</returns>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResponse<ContractDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResponse<ContractDto>>> GetContracts([FromQuery] ContractFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Getting contracts with filter: {@Filter}", filter);
                var result = await _contractService.GetContractsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving contracts");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving contracts"));
            }
        }

        /// <summary>
        /// Get a specific contract by ID
        /// </summary>
        /// <param name="id">Contract GUID</param>
        /// <returns>Contract details</returns>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContractDto>> GetContract(Guid id)
        {
            var contract = await _contractService.GetContractByIdAsync(id);
            if (contract == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Contract with ID {id} not found", statusCode: 404));
            }
            return Ok(contract);
        }

        /// <summary>
        /// Get contract by contract number
        /// </summary>
        /// <param name="contractNumber">Unique contract number (e.g., CNT-2024-001)</param>
        /// <returns>Contract details</returns>
        [HttpGet("by-number/{contractNumber}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContractDto>> GetContractByNumber(string contractNumber)
        {
            var contract = await _contractService.GetContractByNumberAsync(contractNumber);
            if (contract == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Contract {contractNumber} not found", statusCode: 404));
            }
            return Ok(contract);
        }

        /// <summary>
        /// Create a new contract
        /// </summary>
        /// <param name="dto">Contract creation data</param>
        /// <returns>Created contract with ID</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ApiResponse<ContractDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<ContractDto>>> CreateContract([FromBody] CreateContractDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";
            var result = await _contractService.CreateContractAsync(dto, username);

            if (!result.Success)
            {
                return result.StatusCode switch
                {
                    409 => Conflict(result),
                    _ => BadRequest(result)
                };
            }

            return CreatedAtAction(
                nameof(GetContract),
                new { id = result.Data!.Id },
                result);
        }

        /// <summary>
        /// Update contract details (partial update supported)
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <param name="dto">Updated contract data</param>
        /// <returns>Updated contract</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ApiResponse<ContractDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ContractDto>>> UpdateContract(Guid id, [FromBody] UpdateContractDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";
            var result = await _contractService.UpdateContractAsync(id, dto, username);

            if (!result.Success)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    _ => BadRequest(result)
                };
            }

            return Ok(result);
        }

        /// <summary>
        /// Update contract status (Draft→Active, Active→Expired, etc.)
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <param name="dto">Status update with optional reason</param>
        /// <returns>Updated contract</returns>
        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ApiResponse<ContractDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ContractDto>>> UpdateContractStatus(Guid id, [FromBody] UpdateContractStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";
            var result = await _contractService.UpdateContractStatusAsync(id, dto, username);

            if (!result.Success)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    _ => BadRequest(result)
                };
            }

            return Ok(result);
        }

        /// <summary>
        /// Activate a draft contract
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <returns>Activated contract</returns>
        [HttpPost("{id:guid}/activate")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ApiResponse<ContractDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ContractDto>>> ActivateContract(Guid id)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";
            var result = await _contractService.ActivateContractAsync(id, username);

            if (!result.Success)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    _ => BadRequest(result)
                };
            }

            return Ok(result);
        }

        /// <summary>
        /// Expire an active contract
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <returns>Expired contract</returns>
        [HttpPost("{id:guid}/expire")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ApiResponse<ContractDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ContractDto>>> ExpireContract(Guid id)
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";
            var result = await _contractService.ExpireContractAsync(id, username);

            if (!result.Success)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    _ => BadRequest(result)
                };
            }

            return Ok(result);
        }

        /// <summary>
        /// Delete a contract (only Draft contracts with no service requests)
        /// </summary>
        /// <param name="id">Contract ID</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteContract(Guid id)
        {
            var result = await _contractService.DeleteContractAsync(id);

            if (!result.Success)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    _ => BadRequest(result)
                };
            }

            return Ok(result);
        }

        /// <summary>
        /// Get contracts expiring within specified days
        /// </summary>
        /// <param name="days">Number of days threshold (default: 30)</param>
        /// <returns>List of expiring contracts</returns>
        [HttpGet("expiring")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(IEnumerable<ContractDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ContractDto>>> GetExpiringContracts([FromQuery] int days = 30)
        {
            var contracts = await _contractService.GetExpiringContractsAsync(days);
            return Ok(contracts);
        }
    }
}