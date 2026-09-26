using MaintenanceRequestSystemAPI.DTOs.Categories;
using MaintenanceRequestSystemAPI.DTOs.Common;
using MaintenanceRequestSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MaintenanceRequestSystemAPI.Controllers;

[ApiController]
[Route("api/v1/categories")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
[EnableRateLimiting("ApiPolicy")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>Get all categories</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<CategoryDto>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        // Only Admin can see inactive categories
        if (includeInactive && !User.IsInRole("Admin"))
            includeInactive = false;

        var categories = await _categoryService.GetAllCategoriesAsync(includeInactive);
        return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(categories, "Categories retrieved successfully"));
    }

    /// <summary>Get category by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound(ApiResponse.ErrorResponse($"Category with ID {id} not found"));

        return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Category retrieved successfully"));
    }

    /// <summary>Create a new category (Admin only)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildValidationError());

        var category = await _categoryService.CreateCategoryAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = category.Id },
            ApiResponse<CategoryDto>.SuccessResponse(category, "Category created successfully"));
    }

    /// <summary>Update a category (Admin only)</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildValidationError());

        var category = await _categoryService.UpdateCategoryAsync(id, dto);
        if (category == null)
            return NotFound(ApiResponse.ErrorResponse($"Category with ID {id} not found"));

        return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Category updated successfully"));
    }

    /// <summary>Delete a category (Admin only)</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _categoryService.DeleteCategoryAsync(id);
        if (!success)
            return NotFound(ApiResponse.ErrorResponse($"Category with ID {id} not found"));

        return NoContent();
    }

    private ApiResponse BuildValidationError()
    {
        var errors = ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(e => new ValidationError
            {
                Field = x.Key,
                Message = e.ErrorMessage
            }))
            .ToList();
        return ApiResponse.ErrorResponse("Validation failed", errors);
    }
}
