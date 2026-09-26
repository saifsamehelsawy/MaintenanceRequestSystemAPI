using Microsoft.AspNetCore.Identity;

namespace MaintenanceRequestSystemAPI.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public ICollection<MaintenanceRequest> SubmittedRequests { get; set; } = new List<MaintenanceRequest>();
    public ICollection<MaintenanceRequest> AssignedRequests { get; set; } = new List<MaintenanceRequest>();
    public ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();
    public ICollection<RequestHistory> HistoryActions { get; set; } = new List<RequestHistory>();
}
