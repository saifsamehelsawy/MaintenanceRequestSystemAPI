using MaintenanceRequestSystemAPI.Data;
using MaintenanceRequestSystemAPI.DTOs.Common;
using MaintenanceRequestSystemAPI.DTOs.Requests;
using MaintenanceRequestSystemAPI.Helpers;
using MaintenanceRequestSystemAPI.Models;
using MaintenanceRequestSystemAPI.Models.Enums;
using MaintenanceRequestSystemAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceRequestSystemAPI.Services;

public class RequestService : IRequestService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RequestService> _logger;

    public RequestService(ApplicationDbContext context, ILogger<RequestService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<RequestListDto>> GetRequestsAsync(
        RequestFilterDto filter, string userId, string userRole)
    {
        // Validate pagination
        filter.Page = filter.Page < 1 ? 1 : filter.Page;
        filter.PageSize = filter.PageSize < 1 ? 10 : Math.Min(filter.PageSize, 100);

        var query = _context.MaintenanceRequests
            .Where(r => !r.IsDeleted)
            .AsQueryable();

        // Resource-level authorization: Employee sees only their own requests
        if (userRole == "Employee")
            query = query.Where(r => r.EmployeeId == userId);

        // Technician sees only assigned requests
        if (userRole == "Technician")
            query = query.Where(r => r.TechnicianId == userId);

        // Filters (executed in SQL via IQueryable)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(r =>
                r.Title.ToLower().Contains(search) ||
                r.Description.ToLower().Contains(search) ||
                r.RequestNumber.ToLower().Contains(search));
        }

        if (filter.Status.HasValue)
            query = query.Where(r => r.Status == filter.Status.Value);

        if (filter.Priority.HasValue)
            query = query.Where(r => r.Priority == filter.Priority.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(r => r.CategoryId == filter.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(filter.TechnicianId))
            query = query.Where(r => r.TechnicianId == filter.TechnicianId);

        if (filter.DateFrom.HasValue)
            query = query.Where(r => r.CreatedAt >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(r => r.CreatedAt <= filter.DateTo.Value);

        // Sorting (whitelist-based, no SQL injection possible)
        query = (filter.SortBy?.ToLower(), filter.SortDirection?.ToLower()) switch
        {
            ("title", "asc") => query.OrderBy(r => r.Title),
            ("title", _) => query.OrderByDescending(r => r.Title),
            ("status", "asc") => query.OrderBy(r => r.Status),
            ("status", _) => query.OrderByDescending(r => r.Status),
            ("priority", "asc") => query.OrderBy(r => r.Priority),
            ("priority", _) => query.OrderByDescending(r => r.Priority),
            ("updatedat", "asc") => query.OrderBy(r => r.UpdatedAt),
            ("updatedat", _) => query.OrderByDescending(r => r.UpdatedAt),
            (_, "asc") => query.OrderBy(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(r => new RequestListDto
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                Title = r.Title,
                Status = r.Status.ToString(),
                Priority = r.Priority.ToString(),
                CategoryName = r.Category.Name,
                EmployeeName = r.Employee.FullName,
                TechnicianName = r.Technician != null ? r.Technician.FullName : null,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();

        return new PagedResult<RequestListDto>
        {
            Items = items,
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<RequestDetailsDto?> GetRequestByIdAsync(int id, string userId, string userRole)
    {
        if (!await CanAccessRequestAsync(id, userId, userRole))
            return null;

        return await BuildRequestDetailsQuery(id);
    }

    public async Task<RequestDetailsDto> CreateRequestAsync(CreateRequestDto dto, string employeeId)
    {
        var requestNumber = await RequestNumberGenerator.GenerateAsync(_context);

        var request = new MaintenanceRequest
        {
            RequestNumber = requestNumber,
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Priority = dto.Priority,
            EmployeeId = employeeId,
            Status = RequestStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MaintenanceRequests.Add(request);
        await _context.SaveChangesAsync();

        // Create history record
        await AddHistoryAsync(request.Id, RequestAction.RequestCreated,
            "Maintenance request created", employeeId);

        _logger.LogInformation("Request {RequestNumber} created by user {UserId}", requestNumber, employeeId);

        return (await BuildRequestDetailsQuery(request.Id))!;
    }

    public async Task<RequestDetailsDto?> UpdateRequestAsync(
        int id, UpdateRequestDto dto, string userId, string userRole)
    {
        var request = await _context.MaintenanceRequests
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (request == null) return null;

        // Business Rule: Employee can only update their own Pending requests
        if (userRole == "Employee")
        {
            if (request.EmployeeId != userId)
                return null;
            if (request.Status != RequestStatus.Pending)
                return null;
        }

        request.Title = dto.Title;
        request.Description = dto.Description;
        request.CategoryId = dto.CategoryId;
        request.Priority = dto.Priority;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await AddHistoryAsync(id, RequestAction.RequestUpdated,
            "Request details updated", userId);

        _logger.LogInformation("Request {Id} updated by user {UserId}", id, userId);
        return await BuildRequestDetailsQuery(id);
    }

    public async Task<bool> DeleteRequestAsync(int id, string userId, string userRole)
    {
        var request = await _context.MaintenanceRequests
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (request == null) return false;

        // Employee can only delete their own Pending requests
        if (userRole == "Employee" &&
            (request.EmployeeId != userId || request.Status != RequestStatus.Pending))
            return false;

        // Soft delete
        request.IsDeleted = true;
        request.DeletedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        await AddHistoryAsync(id, RequestAction.RequestDeleted,
            "Request soft-deleted", userId);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Request {Id} soft-deleted by user {UserId}", id, userId);
        return true;
    }

    public async Task<RequestDetailsDto?> AssignTechnicianAsync(
        int id, AssignTechnicianDto dto, string adminId)
    {
        var request = await _context.MaintenanceRequests
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (request == null) return null;
        if (request.Status == RequestStatus.Closed) return null;

        request.TechnicianId = dto.TechnicianId;
        request.Status = RequestStatus.Assigned;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await AddHistoryAsync(id, RequestAction.TechnicianAssigned,
            $"Technician assigned to request", adminId);

        _logger.LogInformation("Technician {TechId} assigned to request {ReqId} by admin {AdminId}",
            dto.TechnicianId, id, adminId);

        return await BuildRequestDetailsQuery(id);
    }

    public async Task<RequestDetailsDto?> ChangeStatusAsync(
        int id, ChangeRequestStatusDto dto, string userId, string userRole)
    {
        var request = await _context.MaintenanceRequests
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (request == null) return null;

        // Technician can only change status of assigned requests
        if (userRole == "Technician" && request.TechnicianId != userId)
            return null;

        // Validate status transitions
        if (!IsValidStatusTransition(request.Status, dto.Status, userRole))
            return null;

        request.Status = dto.Status;
        request.UpdatedAt = DateTime.UtcNow;

        if (dto.Status == RequestStatus.Resolved)
        {
            request.ResolvedAt = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(dto.ResolutionNotes))
                request.ResolutionNotes = dto.ResolutionNotes;
        }

        if (dto.Status == RequestStatus.Closed)
            request.ClosedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await AddHistoryAsync(id, RequestAction.StatusChanged,
            $"Status changed to {dto.Status}", userId);

        _logger.LogInformation("Request {Id} status changed to {Status} by {UserId}",
            id, dto.Status, userId);

        return await BuildRequestDetailsQuery(id);
    }

    public async Task<List<CommentDto>> GetCommentsAsync(int requestId, string userId, string userRole)
    {
        if (!await CanAccessRequestAsync(requestId, userId, userRole))
            return new List<CommentDto>();

        return await _context.RequestComments
            .Where(c => c.RequestId == requestId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto
            {
                Id = c.Id,
                CommentText = c.CommentText,
                UserName = c.User.FullName,
                UserId = c.UserId,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<CommentDto?> AddCommentAsync(
        int requestId, AddCommentDto dto, string userId, string userRole)
    {
        if (!await CanAccessRequestAsync(requestId, userId, userRole))
            return null;

        var request = await _context.MaintenanceRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && !r.IsDeleted);

        if (request == null) return null;

        var comment = new RequestComment
        {
            RequestId = requestId,
            UserId = userId,
            CommentText = dto.CommentText,
            CreatedAt = DateTime.UtcNow
        };

        _context.RequestComments.Add(comment);
        await _context.SaveChangesAsync();
        await AddHistoryAsync(requestId, RequestAction.CommentAdded, "Comment added", userId);

        var user = await _context.Users.FindAsync(userId);
        _logger.LogInformation("Comment added to request {ReqId} by user {UserId}", requestId, userId);

        return new CommentDto
        {
            Id = comment.Id,
            CommentText = comment.CommentText,
            UserName = user?.FullName ?? "Unknown",
            UserId = userId,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task<List<HistoryDto>> GetHistoryAsync(int requestId, string userId, string userRole)
    {
        if (!await CanAccessRequestAsync(requestId, userId, userRole))
            return new List<HistoryDto>();

        return await _context.RequestHistories
            .Where(h => h.RequestId == requestId)
            .OrderBy(h => h.CreatedAt)
            .Select(h => new HistoryDto
            {
                Id = h.Id,
                Action = h.Action.ToString(),
                Description = h.Description,
                UserName = h.User != null ? h.User.FullName : "System",
                CreatedAt = h.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> RequestExistsAsync(int id)
        => await _context.MaintenanceRequests.AnyAsync(r => r.Id == id && !r.IsDeleted);

    public async Task<bool> CanAccessRequestAsync(int id, string userId, string userRole)
    {
        if (userRole == "Admin") return true;

        var request = await _context.MaintenanceRequests
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (request == null) return false;

        if (userRole == "Employee") return request.EmployeeId == userId;
        if (userRole == "Technician") return request.TechnicianId == userId;

        return false;
    }

    // ── Private Helpers ────────────────────────────────────────────────

    private async Task<RequestDetailsDto?> BuildRequestDetailsQuery(int id)
    {
        return await _context.MaintenanceRequests
            .Where(r => r.Id == id && !r.IsDeleted)
            .Select(r => new RequestDetailsDto
            {
                Id = r.Id,
                RequestNumber = r.RequestNumber,
                Title = r.Title,
                Description = r.Description,
                Status = r.Status.ToString(),
                Priority = r.Priority.ToString(),
                ResolutionNotes = r.ResolutionNotes,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                ResolvedAt = r.ResolvedAt,
                ClosedAt = r.ClosedAt,
                Category = new CategorySummaryDto { Id = r.Category.Id, Name = r.Category.Name },
                Employee = new UserSummaryDto
                {
                    Id = r.Employee.Id,
                    FullName = r.Employee.FullName,
                    Email = r.Employee.Email!,
                    Department = r.Employee.Department
                },
                Technician = r.Technician != null ? new UserSummaryDto
                {
                    Id = r.Technician.Id,
                    FullName = r.Technician.FullName,
                    Email = r.Technician.Email!,
                    Department = r.Technician.Department
                } : null,
                Comments = r.Comments.OrderBy(c => c.CreatedAt).Select(c => new CommentDto
                {
                    Id = c.Id,
                    CommentText = c.CommentText,
                    UserName = c.User.FullName,
                    UserId = c.UserId,
                    CreatedAt = c.CreatedAt
                }).ToList(),
                History = r.History.OrderBy(h => h.CreatedAt).Select(h => new HistoryDto
                {
                    Id = h.Id,
                    Action = h.Action.ToString(),
                    Description = h.Description,
                    UserName = h.User != null ? h.User.FullName : "System",
                    CreatedAt = h.CreatedAt
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    private async Task AddHistoryAsync(int requestId, RequestAction action,
        string description, string? userId)
    {
        _context.RequestHistories.Add(new RequestHistory
        {
            RequestId = requestId,
            Action = action,
            Description = description,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    private static bool IsValidStatusTransition(
        RequestStatus current, RequestStatus target, string userRole)
    {
        return (current, target) switch
        {
            (RequestStatus.Pending, RequestStatus.Assigned) => true,        // Admin assigns
            (RequestStatus.Pending, RequestStatus.Closed) => userRole == "Admin",
            (RequestStatus.Assigned, RequestStatus.InProgress) => true,     // Technician starts
            (RequestStatus.InProgress, RequestStatus.Resolved) => true,     // Technician resolves
            (RequestStatus.Resolved, RequestStatus.Closed) => true,         // Admin/Employee closes
            _ => false
        };
    }
}
