using System.Security.Claims;
using MaintenanceRequestSystemAPI.DTOs.Common;
using MaintenanceRequestSystemAPI.DTOs.Dashboard;
using MaintenanceRequestSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MaintenanceRequestSystemAPI.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
[EnableRateLimiting("ApiPolicy")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>Get Admin dashboard statistics</summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AdminDashboardDto>), 200)]
    public async Task<IActionResult> GetAdminDashboard()
    {
        var data = await _dashboardService.GetAdminDashboardAsync();
        return Ok(ApiResponse<AdminDashboardDto>.SuccessResponse(data, "Admin dashboard retrieved"));
    }

    /// <summary>Get Technician dashboard statistics</summary>
    [HttpGet("technician")]
    [Authorize(Roles = "Technician")]
    [ProducesResponseType(typeof(ApiResponse<TechnicianDashboardDto>), 200)]
    public async Task<IActionResult> GetTechnicianDashboard()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var data = await _dashboardService.GetTechnicianDashboardAsync(userId);
        return Ok(ApiResponse<TechnicianDashboardDto>.SuccessResponse(data, "Technician dashboard retrieved"));
    }

    /// <summary>Get Employee dashboard statistics</summary>
    [HttpGet("employee")]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeDashboardDto>), 200)]
    public async Task<IActionResult> GetEmployeeDashboard()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        var data = await _dashboardService.GetEmployeeDashboardAsync(userId);
        return Ok(ApiResponse<EmployeeDashboardDto>.SuccessResponse(data, "Employee dashboard retrieved"));
    }
}
