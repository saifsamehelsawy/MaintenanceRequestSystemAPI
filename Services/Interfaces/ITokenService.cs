using MaintenanceRequestSystemAPI.Models;

namespace MaintenanceRequestSystemAPI.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
    DateTime GetExpirationDate();
}
