using System.ComponentModel.DataAnnotations;
using MaintenanceRequestSystemAPI.Models.Enums;

namespace MaintenanceRequestSystemAPI.DTOs.Requests;

// ── Input DTOs ──────────────────────────────────────────────────

public class CreateRequestDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Priority is required")]
    public Priority Priority { get; set; } = Priority.Medium;
}

public class UpdateRequestDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Priority is required")]
    public Priority Priority { get; set; }
}

public class AssignTechnicianDto
{
    [Required(ErrorMessage = "Technician ID is required")]
    public string TechnicianId { get; set; } = string.Empty;
}

public class ChangeRequestStatusDto
{
    [Required(ErrorMessage = "Status is required")]
    public RequestStatus Status { get; set; }

    [MaxLength(2000)]
    public string? ResolutionNotes { get; set; }
}

public class AddCommentDto
{
    [Required(ErrorMessage = "Comment text is required")]
    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters")]
    public string CommentText { get; set; } = string.Empty;
}

public class RequestFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public RequestStatus? Status { get; set; }
    public Priority? Priority { get; set; }
    public int? CategoryId { get; set; }
    public string? TechnicianId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SortBy { get; set; } = "createdAt";
    public string? SortDirection { get; set; } = "desc";
}

// ── Output DTOs ──────────────────────────────────────────────────

public class RequestListDto
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string? TechnicianName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RequestDetailsDto
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? ResolutionNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public CategorySummaryDto Category { get; set; } = new();
    public UserSummaryDto Employee { get; set; } = new();
    public UserSummaryDto? Technician { get; set; }
    public List<CommentDto> Comments { get; set; } = new();
    public List<HistoryDto> History { get; set; } = new();
}

public class CommentDto
{
    public int Id { get; set; }
    public string CommentText { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class HistoryDto
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CategorySummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class UserSummaryDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Department { get; set; }
}
