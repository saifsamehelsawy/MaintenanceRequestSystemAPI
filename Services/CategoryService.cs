using MaintenanceRequestSystemAPI.Data;
using MaintenanceRequestSystemAPI.DTOs.Categories;
using MaintenanceRequestSystemAPI.Models;
using MaintenanceRequestSystemAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MaintenanceRequestSystemAPI.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ApplicationDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync(bool includeInactive = false)
    {
        var query = _context.Categories.AsQueryable();
        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                RequestCount = c.Requests.Count(r => !r.IsDeleted)
            })
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt,
                RequestCount = c.Requests.Count(r => !r.IsDeleted)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category '{Name}' created with ID {Id}", category.Name, category.Id);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            RequestCount = 0
        };
    }

    public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return null;

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Category {Id} updated", id);

        return await GetCategoryByIdAsync(id);
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Requests)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return false;

        // Check if category has active requests
        if (category.Requests.Any(r => !r.IsDeleted))
        {
            // Soft delete by deactivating
            category.IsActive = false;
            await _context.SaveChangesAsync();
            _logger.LogWarning("Category {Id} deactivated (has active requests)", id);
            return true;
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Category {Id} deleted", id);
        return true;
    }

    public async Task<bool> CategoryExistsAsync(int id)
        => await _context.Categories.AnyAsync(c => c.Id == id && c.IsActive);
}
