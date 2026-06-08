using GLMS.API.Models;
using GLMS.API.Models.DTOs;
using GLMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GLMS.API.Controllers
{
    /// <summary>
    /// Manages service requests linked to logistics contracts
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestService _serviceRequestService;
        private readonly ILogger<ServiceRequestsController> _logger;

        public ServiceRequestsController(IServiceRequestService serviceRequestService, ILogger<ServiceRequestsController> logger)
        {
            _serviceRequestService = serviceRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Get all service requests with filtering and pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PagedResponse<ServiceRequestDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResponse<ServiceRequestDto>>> GetServiceRequests([FromQuery] ServiceRequestFilterDto filter)
        {
            var result = await _serviceRequestService.GetServiceRequestsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Get service request by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ServiceRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ServiceRequestDto>> GetServiceRequest(Guid id)
        {
            var request = await _serviceRequestService.GetServiceRequestByIdAsync(id);
            if (request == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse($"Service request {id} not found", statusCode: 404));
            }
            return Ok(request);
        }

        /// <summary>
        /// Create a new service request (linked to active contract)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,User")]
        [ProducesResponseType(typeof(ApiResponse<ServiceRequestDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ServiceRequestDto>>> CreateServiceRequest([FromBody] CreateServiceRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";
            var result = await _serviceRequestService.CreateServiceRequestAsync(dto, username);

            if (!result.Success)
            {
                return result.StatusCode switch
                {
                    404 => NotFound(result),
                    _ => BadRequest(result)
                };
            }

            return CreatedAtAction(
                nameof(GetServiceRequest),
                new { id = result.Data!.Id },
                result);
        }

        /// <summary>
        /// Update service request status (Pending→Approved, InProgress→Completed, etc.)
        /// </summary>
        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ApiResponse<ServiceRequestDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<ServiceRequestDto>>> UpdateStatus(Guid id, [FromBody] UpdateServiceRequestStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            }

            var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "system";
            var result = await _serviceRequestService.UpdateServiceRequestStatusAsync(id, dto, username);

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
        /// Delete a pending service request
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteServiceRequest(Guid id)
        {
            var result = await _serviceRequestService.DeleteServiceRequestAsync(id);

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
        /// Get all pending service requests
        /// </summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(IEnumerable<ServiceRequestDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ServiceRequestDto>>> GetPendingRequests()
        {
            var requests = await _serviceRequestService.GetPendingServiceRequestsAsync();
            return Ok(requests);
        }
    }
}