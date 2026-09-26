using MaintenanceRequestSystemAPI.DTOs.Common;
using MaintenanceRequestSystemAPI.DTOs.Users;
using MaintenanceRequestSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MaintenanceRequestSystemAPI.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[Produces("application/json")]
[EnableRateLimiting("ApiPolicy")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>Get all users (Admin only)</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(ApiResponse<List<UserDto>>.SuccessResponse(users, "Users retrieved successfully"));
    }

    /// <summary>Get user by ID (Admin only)</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponse.ErrorResponse($"User not found"));

        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User retrieved successfully"));
    }

    /// <summary>Update user information (Admin only)</summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildValidationError());

        var user = await _userService.UpdateUserAsync(id, dto);
        if (user == null)
            return NotFound(ApiResponse.ErrorResponse("User not found or update failed"));

        return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User updated successfully"));
    }

    /// <summary>Activate a user account (Admin only)</summary>
    [HttpPut("{id}/activate")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Activate(string id)
    {
        var success = await _userService.ActivateUserAsync(id);
        if (!success)
            return NotFound(ApiResponse.ErrorResponse("User not found"));

        return Ok(ApiResponse.SuccessResponse("User activated successfully"));
    }

    /// <summary>Deactivate a user account (Admin only)</summary>
    [HttpPut("{id}/deactivate")]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Deactivate(string id)
    {
        var success = await _userService.DeactivateUserAsync(id);
        if (!success)
            return NotFound(ApiResponse.ErrorResponse("User not found"));

        return Ok(ApiResponse.SuccessResponse("User deactivated successfully"));
    }

    /// <summary>Get all technicians (Admin only)</summary>
    [HttpGet("technicians")]
    [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), 200)]
    public async Task<IActionResult> GetTechnicians()
    {
        var technicians = await _userService.GetTechniciansAsync();
        return Ok(ApiResponse<List<UserDto>>.SuccessResponse(technicians, "Technicians retrieved successfully"));
    }

    private ApiResponse BuildValidationError()
    {
        var errors = ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(e => new ValidationError
            {
                Field = x.Key,
                Message = e.ErrorMessage
            }))
            .ToList();
        return ApiResponse.ErrorResponse("Validation failed", errors);
    }
}
