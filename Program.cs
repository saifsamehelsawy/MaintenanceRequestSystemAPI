using MaintenanceRequestSystemAPI.Extensions;
using MaintenanceRequestSystemAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddIdentityServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddRateLimitingPolicy(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ── App Pipeline ──────────────────────────────────────────────────────────
var app = builder.Build();

// Global Exception Handler (must be first)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger — default route
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Maintenance Request System API v1");
    c.RoutePrefix = string.Empty; // Swagger at root "/"
    c.DisplayRequestDuration();
});

app.UseHttpsRedirection();

// CORS before Auth
app.UseCors(app.Environment.IsDevelopment() ? "DevelopmentCors" : "ProductionCors");

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ── Seed Database ─────────────────────────────────────────────────────────
await SeedData.InitializeAsync(app.Services);

app.Run();

// ── Seed Data ─────────────────────────────────────────────────────────────
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var userManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<MaintenanceRequestSystemAPI.Models.ApplicationUser>>();
            var roleManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
            var logger = services.GetRequiredService<ILogger<Program>>();

            // Create roles
            string[] roles = ["Admin", "Technician", "Employee"];
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(role));
                    logger.LogInformation("Role created: {Role}", role);
                }
            }

            // Seed Admin
            await SeedUserAsync(userManager, logger,
                email: "admin@maintenance.com",
                fullName: "System Administrator",
                department: "IT",
                password: "Admin@123",
                role: "Admin");

            // Seed Technician
            await SeedUserAsync(userManager, logger,
                email: "tech@maintenance.com",
                fullName: "Ahmed Hassan",
                department: "Maintenance",
                password: "Tech@123",
                role: "Technician");

            // Seed Employee
            await SeedUserAsync(userManager, logger,
                email: "emp@maintenance.com",
                fullName: "Sara Mohamed",
                department: "HR",
                password: "Emp@123",
                role: "Employee");

            logger.LogInformation("Database seeded successfully");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error seeding database");
        }
    }

    private static async Task SeedUserAsync(
        Microsoft.AspNetCore.Identity.UserManager<MaintenanceRequestSystemAPI.Models.ApplicationUser> userManager,
        ILogger logger,
        string email, string fullName, string department, string password, string role)
    {
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new MaintenanceRequestSystemAPI.Models.ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                Department = department,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
                logger.LogInformation("Seeded user: {Email} ({Role})", email, role);
            }
            else
            {
                logger.LogError("Failed to seed user {Email}: {Errors}", email,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
