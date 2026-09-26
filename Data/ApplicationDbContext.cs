using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MaintenanceRequestSystemAPI.Models;
using MaintenanceRequestSystemAPI.Models.Enums;

namespace MaintenanceRequestSystemAPI.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<RequestComment> RequestComments { get; set; }
    public DbSet<RequestHistory> RequestHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // MaintenanceRequest → Employee (restrict)
        builder.Entity<MaintenanceRequest>()
            .HasOne(r => r.Employee)
            .WithMany(u => u.SubmittedRequests)
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // MaintenanceRequest → Technician (restrict, nullable)
        builder.Entity<MaintenanceRequest>()
            .HasOne(r => r.Technician)
            .WithMany(u => u.AssignedRequests)
            .HasForeignKey(r => r.TechnicianId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // RequestComment → User (restrict)
        builder.Entity<RequestComment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // RequestHistory → User (set null on delete)
        builder.Entity<RequestHistory>()
            .HasOne(h => h.User)
            .WithMany(u => u.HistoryActions)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes
        builder.Entity<MaintenanceRequest>()
            .HasIndex(r => r.RequestNumber)
            .IsUnique();

        builder.Entity<MaintenanceRequest>()
            .HasIndex(r => new { r.IsDeleted, r.Status });

        builder.Entity<MaintenanceRequest>()
            .HasIndex(r => r.EmployeeId);

        builder.Entity<MaintenanceRequest>()
            .HasIndex(r => r.TechnicianId);

        // Enum → string conversions
        builder.Entity<MaintenanceRequest>()
            .Property(r => r.Status)
            .HasConversion<string>();

        builder.Entity<MaintenanceRequest>()
            .Property(r => r.Priority)
            .HasConversion<string>();

        builder.Entity<RequestHistory>()
            .Property(h => h.Action)
            .HasConversion<string>();

        // Seed Categories
        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electrical", Description = "Electrical systems and wiring issues", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 2, Name = "Plumbing", Description = "Water and drainage issues", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 3, Name = "HVAC", Description = "Heating, ventilation, and air conditioning", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 4, Name = "IT Equipment", Description = "Computers, printers, and network equipment", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Category { Id = 5, Name = "Furniture", Description = "Furniture repair and replacement", IsActive = true, CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
