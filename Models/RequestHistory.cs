using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MaintenanceRequestSystemAPI.Models.Enums;

namespace MaintenanceRequestSystemAPI.Models;

public class RequestHistory
{
    public int Id { get; set; }

    public RequestAction Action { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int RequestId { get; set; }
    public string? UserId { get; set; }

    // Navigation
    [ForeignKey(nameof(RequestId))]
    public MaintenanceRequest Request { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }
}
