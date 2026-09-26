namespace MaintenanceRequestSystemAPI.DTOs.Dashboard;

public class StatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class PriorityCountDto
{
    public string Priority { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class CategoryCountDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RecentRequestDto
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AdminDashboardDto
{
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int AssignedRequests { get; set; }
    public int InProgressRequests { get; set; }
    public int ResolvedRequests { get; set; }
    public int ClosedRequests { get; set; }
    public int CriticalRequests { get; set; }
    public int TotalUsers { get; set; }
    public int TotalTechnicians { get; set; }
    public List<StatusCountDto> RequestsByStatus { get; set; } = new();
    public List<PriorityCountDto> RequestsByPriority { get; set; } = new();
    public List<CategoryCountDto> RequestsByCategory { get; set; } = new();
    public List<RecentRequestDto> RecentRequests { get; set; } = new();
}

public class TechnicianDashboardDto
{
    public int AssignedToMe { get; set; }
    public int InProgressByMe { get; set; }
    public int ResolvedByMe { get; set; }
    public int ClosedByMe { get; set; }
    public List<RecentRequestDto> MyActiveRequests { get; set; } = new();
    public List<RecentRequestDto> RecentlyResolved { get; set; } = new();
}

public class EmployeeDashboardDto
{
    public int MyTotalRequests { get; set; }
    public int MyPendingRequests { get; set; }
    public int MyInProgressRequests { get; set; }
    public int MyResolvedRequests { get; set; }
    public int MyClosedRequests { get; set; }
    public List<RecentRequestDto> MyRecentRequests { get; set; } = new();
}
