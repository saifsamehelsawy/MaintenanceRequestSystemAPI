using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MaintenanceRequestSystemAPI.Models.Enums;

namespace MaintenanceRequestSystemAPI.Models;

public class MaintenanceRequest
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string RequestNumber { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public Priority Priority { get; set; } = Priority.Medium;

    [MaxLength(2000)]
    public string? ResolutionNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Foreign Keys
    public int CategoryId { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string? TechnicianId { get; set; }

    // Navigation
    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    [ForeignKey(nameof(EmployeeId))]
    public ApplicationUser Employee { get; set; } = null!;

    [ForeignKey(nameof(TechnicianId))]
    public ApplicationUser? Technician { get; set; }

    public ICollection<RequestComment> Comments { get; set; } = new List<RequestComment>();
    public ICollection<RequestHistory> History { get; set; } = new List<RequestHistory>();
}
