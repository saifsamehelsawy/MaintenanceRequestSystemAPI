using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MaintenanceRequestSystemAPI.Models;

public class RequestComment
{
    public int Id { get; set; }

    [Required, MaxLength(1000)]
    public string CommentText { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int RequestId { get; set; }
    public string UserId { get; set; } = string.Empty;

    // Navigation
    [ForeignKey(nameof(RequestId))]
    public MaintenanceRequest Request { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;
}
