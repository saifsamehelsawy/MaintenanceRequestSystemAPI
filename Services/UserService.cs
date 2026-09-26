using MaintenanceRequestSystemAPI.Data;
using MaintenanceRequestSystemAPI.DTOs.Users;
using MaintenanceRequestSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MaintenanceRequestSystemAPI.Models;

namespace MaintenanceRequestSystemAPI.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<UserService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var result = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var requestCount = await _context.MaintenanceRequests
                .CountAsync(r => r.EmployeeId == user.Id && !r.IsDeleted);

            result.Add(MapToDto(user, roles, requestCount));
        }
        return result;
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var requestCount = await _context.MaintenanceRequests
            .CountAsync(r => r.EmployeeId == id && !r.IsDeleted);

        return MapToDto(user, roles, requestCount);
    }

    public async Task<UserDto?> UpdateUserAsync(string id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return null;

        user.FullName = dto.FullName;
        user.Department = dto.Department;
        user.PhoneNumber = dto.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return null;

        _logger.LogInformation("User {Id} updated", id);
        return await GetUserByIdAsync(id);
    }

    public async Task<bool> ActivateUserAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsActive = true;
        await _context.SaveChangesAsync();
        _logger.LogInformation("User {Id} activated", id);
        return true;
    }

    public async Task<bool> DeactivateUserAsync(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        await _context.SaveChangesAsync();
        _logger.LogInformation("User {Id} deactivated", id);
        return true;
    }

    public async Task<List<UserDto>> GetTechniciansAsync()
    {
        var technicians = await _userManager.GetUsersInRoleAsync("Technician");
        var result = new List<UserDto>();

        foreach (var user in technicians.Where(u => u.IsActive).OrderBy(u => u.FullName))
        {
            var roles = await _userManager.GetRolesAsync(user);
            var requestCount = await _context.MaintenanceRequests
                .CountAsync(r => r.TechnicianId == user.Id && !r.IsDeleted);
            result.Add(MapToDto(user, roles, requestCount));
        }
        return result;
    }

    public async Task<bool> UserExistsAsync(string id)
        => await _context.Users.AnyAsync(u => u.Id == id);

    private static UserDto MapToDto(ApplicationUser user, IList<string> roles, int requestCount) =>
        new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Department = user.Department,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = roles.ToList(),
            TotalRequests = requestCount
        };
}
