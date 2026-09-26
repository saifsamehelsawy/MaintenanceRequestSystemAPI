using MaintenanceRequestSystemAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceRequestSystemAPI.Helpers;

public static class RequestNumberGenerator
{
    public static async Task<string> GenerateAsync(ApplicationDbContext context)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"REQ-{year}-";

        var lastRequest = await context.MaintenanceRequests
            .Where(r => r.RequestNumber.StartsWith(prefix))
            .OrderByDescending(r => r.RequestNumber)
            .Select(r => r.RequestNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastRequest != null)
        {
            var parts = lastRequest.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int last))
                nextNumber = last + 1;
        }

        return $"{prefix}{nextNumber:D4}";
    }
}
