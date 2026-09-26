using MaintenanceRequestSystemAPI.Data;
using MaintenanceRequestSystemAPI.DTOs.Dashboard;
using MaintenanceRequestSystemAPI.Models.Enums;
using MaintenanceRequestSystemAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceRequestSystemAPI.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync()
    {
        var requests = _context.MaintenanceRequests.Where(r => !r.IsDeleted);

        var byStatus = await requests
            .GroupBy(r => r.Status)
            .Select(g => new StatusCountDto { Status = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var byPriority = await requests
            .GroupBy(r => r.Priority)
            .Select(g => new PriorityCountDto { Priority = g.Key.ToString(), Count = g.Count() })
            .ToListAsync();

        var byCategory = await requests
            .GroupBy(r => r.Category.Name)
            .Select(g => new CategoryCountDto { CategoryName = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToListAsync();

        var recentRequests = await requests
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .Select(r => new RecentRequestDto
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                Title = r.Title,
                Status = r.Status.ToString(),
                Priority = r.Priority.ToString(),
                EmployeeName = r.Employee.FullName,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var totalUsers = await _context.Users.CountAsync();
        var totalTechnicians = await (
            from ur in _context.UserRoles
            join r in _context.Roles on ur.RoleId equals r.Id
            where r.Name == "Technician"
            select ur).CountAsync();

        return new AdminDashboardDto
        {
            TotalRequests = await requests.CountAsync(),
            PendingRequests = await requests.CountAsync(r => r.Status == RequestStatus.Pending),
            AssignedRequests = await requests.CountAsync(r => r.Status == RequestStatus.Assigned),
            InProgressRequests = await requests.CountAsync(r => r.Status == RequestStatus.InProgress),
            ResolvedRequests = await requests.CountAsync(r => r.Status == RequestStatus.Resolved),
            ClosedRequests = await requests.CountAsync(r => r.Status == RequestStatus.Closed),
            CriticalRequests = await requests.CountAsync(r => r.Priority == Priority.Critical),
            TotalUsers = totalUsers,
            TotalTechnicians = totalTechnicians,
            RequestsByStatus = byStatus,
            RequestsByPriority = byPriority,
            RequestsByCategory = byCategory,
            RecentRequests = recentRequests
        };
    }

    public async Task<TechnicianDashboardDto> GetTechnicianDashboardAsync(string technicianId)
    {
        var myRequests = _context.MaintenanceRequests
            .Where(r => !r.IsDeleted && r.TechnicianId == technicianId);

        var activeRequests = await myRequests
            .Where(r => r.Status == RequestStatus.Assigned || r.Status == RequestStatus.InProgress)
            .OrderByDescending(r => r.UpdatedAt)
            .Take(10)
            .Select(r => new RecentRequestDto
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                Title = r.Title,
                Status = r.Status.ToString(),
                Priority = r.Priority.ToString(),
                EmployeeName = r.Employee.FullName,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var recentlyResolved = await myRequests
            .Where(r => r.Status == RequestStatus.Resolved || r.Status == RequestStatus.Closed)
            .OrderByDescending(r => r.ResolvedAt)
            .Take(5)
            .Select(r => new RecentRequestDto
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                Title = r.Title,
                Status = r.Status.ToString(),
                Priority = r.Priority.ToString(),
                EmployeeName = r.Employee.FullName,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new TechnicianDashboardDto
        {
            AssignedToMe = await myRequests.CountAsync(r => r.Status == RequestStatus.Assigned),
            InProgressByMe = await myRequests.CountAsync(r => r.Status == RequestStatus.InProgress),
            ResolvedByMe = await myRequests.CountAsync(r => r.Status == RequestStatus.Resolved),
            ClosedByMe = await myRequests.CountAsync(r => r.Status == RequestStatus.Closed),
            MyActiveRequests = activeRequests,
            RecentlyResolved = recentlyResolved
        };
    }

    public async Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(string employeeId)
    {
        var myRequests = _context.MaintenanceRequests
            .Where(r => !r.IsDeleted && r.EmployeeId == employeeId);

        var recentRequests = await myRequests
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .Select(r => new RecentRequestDto
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                Title = r.Title,
                Status = r.Status.ToString(),
                Priority = r.Priority.ToString(),
                EmployeeName = r.Employee.FullName,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return new EmployeeDashboardDto
        {
            MyTotalRequests = await myRequests.CountAsync(),
            MyPendingRequests = await myRequests.CountAsync(r => r.Status == RequestStatus.Pending),
            MyInProgressRequests = await myRequests.CountAsync(r =>
                r.Status == RequestStatus.Assigned || r.Status == RequestStatus.InProgress),
            MyResolvedRequests = await myRequests.CountAsync(r => r.Status == RequestStatus.Resolved),
            MyClosedRequests = await myRequests.CountAsync(r => r.Status == RequestStatus.Closed),
            MyRecentRequests = recentRequests
        };
    }
}
