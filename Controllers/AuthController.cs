using MaintenanceRequestSystemAPI.DTOs.Auth;
using MaintenanceRequestSystemAPI.DTOs.Common;
using MaintenanceRequestSystemAPI.Models;
using MaintenanceRequestSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MaintenanceRequestSystemAPI.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>Register a new user</summary>
    [HttpPost("register")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildValidationError());

        // Only allow Employee and Technician self-registration
        if (dto.Role == "Admin")
            return BadRequest(ApiResponse.ErrorResponse("Admin accounts cannot be self-registered"));

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return Conflict(ApiResponse.ErrorResponse("Email is already registered"));

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            Department = dto.Department,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new ValidationError
            {
                Field = "Password",
                Message = e.Description
            }).ToList();
            return BadRequest(ApiResponse.ErrorResponse("Registration failed", errors));
        }

        var role = dto.Role is "Employee" or "Technician" ? dto.Role : "Employee";
        await _userManager.AddToRoleAsync(user, role);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);
        var expiresAt = _tokenService.GetExpirationDate();

        _logger.LogInformation("New user registered: {Email} with role {Role}", dto.Email, role);

        var response = new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserInfoDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Department = user.Department,
                Roles = roles.ToList()
            }
        };

        return StatusCode(201, ApiResponse<AuthResponseDto>.SuccessResponse(response, "Registration successful"));
    }

    /// <summary>Login and receive JWT token</summary>
    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 401)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildValidationError());

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", dto.Email);
            return Unauthorized(ApiResponse.ErrorResponse("Invalid email or password"));
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt by deactivated user: {Email}", dto.Email);
            return Unauthorized(ApiResponse.ErrorResponse("Account is deactivated. Contact administrator."));
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for email: {Email}", dto.Email);
            if (result.IsLockedOut)
                return StatusCode(429, ApiResponse.ErrorResponse("Account locked. Try again later."));
            return Unauthorized(ApiResponse.ErrorResponse("Invalid email or password"));
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);
        var expiresAt = _tokenService.GetExpirationDate();

        _logger.LogInformation("User logged in: {Email}", dto.Email);

        var response = new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new UserInfoDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Department = user.Department,
                Roles = roles.ToList()
            }
        };

        return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(response, "Login successful"));
    }

    /// <summary>Logout (client-side token invalidation)</summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), 200)]
    public IActionResult Logout()
    {
        // JWT is stateless — client simply discards the token
        _logger.LogInformation("User logged out: {UserId}", User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
        return Ok(ApiResponse.SuccessResponse("Logged out successfully"));
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
