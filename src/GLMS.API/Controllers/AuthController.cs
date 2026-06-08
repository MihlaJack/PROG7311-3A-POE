using GLMS.API.Models;
using GLMS.API.Models.DTOs;
using GLMS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.API.Controllers
{
    /// <summary>
    /// Authentication and user management endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Login with username and password to receive JWT token
        /// </summary>
        /// <param name="request">Login credentials</param>
        /// <returns>JWT token and user details</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            }

            var response = await _authService.LoginAsync(request);

            if (response == null)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(
                    "Invalid username or password",
                    statusCode: 401));
            }

            return Ok(response);
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <returns>Created user</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<User>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<User>>> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid request data",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));
            }

            var result = await _authService.RegisterAsync(request);

            if (!result.Success)
            {
                return result.StatusCode switch
                {
                    409 => Conflict(result),
                    _ => BadRequest(result)
                };
            }

            return CreatedAtAction(nameof(GetUser), new { id = result.Data!.Id }, result);
        }

        /// <summary>
        /// Get current authenticated user details
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<User>> GetCurrentUser()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Not authenticated", statusCode: 401));
            }

            var user = await _authService.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found", statusCode: 404));
            }

            // Don't return password hash
            user.PasswordHash = "[REDACTED]";
            return Ok(user);
        }

        /// <summary>
        /// Get user by ID (Admin only)
        /// </summary>
        [HttpGet("users/{id:guid}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User not found", statusCode: 404));
            }

            user.PasswordHash = "[REDACTED]";
            return Ok(user);
        }
    }
}