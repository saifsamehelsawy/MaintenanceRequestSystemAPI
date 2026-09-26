using MaintenanceRequestSystemAPI.DTOs.Users;

namespace MaintenanceRequestSystemAPI.Services.Interfaces;

public interface IUserService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<UserDto?> UpdateUserAsync(string id, UpdateUserDto dto);
    Task<bool> ActivateUserAsync(string id);
    Task<bool> DeactivateUserAsync(string id);
    Task<List<UserDto>> GetTechniciansAsync();
    Task<bool> UserExistsAsync(string id);
}
