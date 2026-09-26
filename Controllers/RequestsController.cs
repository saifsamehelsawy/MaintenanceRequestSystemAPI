using System.Security.Claims;
using MaintenanceRequestSystemAPI.DTOs.Common;
using MaintenanceRequestSystemAPI.DTOs.Requests;
using MaintenanceRequestSystemAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MaintenanceRequestSystemAPI.Controllers;

[ApiController]
[Route("api/v1/requests")]
[Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
[EnableRateLimiting("ApiPolicy")]
public class RequestsController : ControllerBase
{
    private readonly IRequestService _requestService;
    private readonly ICategoryService _categoryService;
    private readonly IUserService _userService;
    private readonly ILogger<RequestsController> _logger;

    public RequestsController(
        IRequestService requestService,
        ICategoryService categoryService,
        IUserService userService,
        ILogger<RequestsController> logger)
    {
        _requestService = requestService;
        _categoryService = categoryService;
        _userService = userService;
        _logger = logger;
    }

    /// <summary>Get paginated list of requests with filtering and sorting</summary>
    /// <remarks>
    /// Employee: sees only their own requests
    /// Technician: sees only assigned requests
    /// Admin: sees all requests
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RequestListDto>>), 200)]
    public async Task<IActionResult> GetRequests([FromQuery] RequestFilterDto filter)
    {
        var (userId, userRole) = GetUserContext();
        var result = await _requestService.GetRequestsAsync(filter, userId, userRole);
        return Ok(ApiResponse<PagedResult<RequestListDto>>.SuccessResponse(result, "Requests retrieved successfully"));
    }

    /// <summary>Get request details by ID</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RequestDetailsDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var (userId, userRole) = GetUserContext();

        if (!await _requestService.RequestExistsAsync(id))
            return NotFound(ApiResponse.ErrorResponse($"Request with ID {id} not found"));

        var result = await _requestService.GetRequestByIdAsync(id, userId, userRole);
        if (result == null)
            return Forbid();

        return Ok(ApiResponse<RequestDetailsDto>.SuccessResponse(result, "Request retrieved successfully"));
    }

    /// <summary>Create a new maintenance request</summary>
    [HttpPost]
    [Authorize(Roles = "Employee")]
    [ProducesResponseType(typeof(ApiResponse<RequestDetailsDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    public async Task<IActionResult> Create([FromBody] CreateRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildValidationError("Validation failed"));

        if (!await _categoryService.CategoryExistsAsync(dto.CategoryId))
            return BadRequest(ApiResponse.ErrorResponse("Category not found or inactive"));

        var userId = GetUserId();
        var result = await _requestService.CreateRequestAsync(dto, userId);

        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<RequestDetailsDto>.SuccessResponse(result, "Request created successfully"));
    }

    /// <summary>Update a request (Employee: own Pending requests only)</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<RequestDetailsDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildValidationError("Validation failed"));

        if (!await _requestService.RequestExistsAsync(id))
            return NotFound(ApiResponse.ErrorResponse($"Request with ID {id} not found"));

        if (!await _categoryService.CategoryExistsAsync(dto.CategoryId))
            return BadRequest(ApiResponse.ErrorResponse("Category not found or inactive"));

        var (userId, userRole) = GetUserContext();
        var result = await _requestService.UpdateRequestAsync(id, dto, userId, userRole);

        if (result == null)
            return StatusCode(403, ApiResponse.ErrorResponse(
                "You cannot update this request. It may belong to another user or not be in Pending status."));

        return Ok(ApiResponse<RequestDetailsDto>.SuccessResponse(result, "Request updated successfully"));
    }

    /// <summary>Delete (soft-delete) a request</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _requestService.RequestExistsAsync(id))
            return NotFound(ApiResponse.ErrorResponse($"Request with ID {id} not found"));

        var (userId, userRole) = GetUserContext();
        var success = await _requestService.DeleteRequestAsync(id, userId, userRole);

        if (!success)
            return StatusCode(403, ApiResponse.ErrorResponse(
                "You cannot delete this request."));

        return NoContent();
    }

    /// <summary>Assign a technician to a request (Admin only)</summary>
    [HttpPost("{id:int}/assign")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<RequestDetailsDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 409)]
    public async Task<IActionResult> AssignTechnician(int id, [FromBody] AssignTechnicianDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildValidationError("Validation failed"));

        if (!await _requestService.RequestExistsAsync(id))
            return NotFound(ApiResponse.ErrorResponse($"Request with ID {id} not found"));

        if (!await _userService.UserExistsAsync(dto.TechnicianId))
            return NotFound(ApiResponse.ErrorResponse("Technician not found"));

        var adminId = GetUserId();
        var result = await _requestService.AssignTechnicianAsync(id, dto, adminId);

        if (result == null)
            return Conflict(ApiResponse.ErrorResponse("Cannot assign technician to a closed request"));

        _logger.LogInformation("Admin {AdminId} assigned technician {TechId} to request {ReqId}",
            adminId, dto.TechnicianId, id);

        return Ok(ApiResponse<RequestDetailsDto>.SuccessResponse(result, "Technician assigned successfully"));
    }

    /// <summary>Change request status (Technician/Admin)</summary>
    [HttpPut("{id:int}/status")]
    [Authorize(Roles = "Technician,Admin")]
    [ProducesResponseType(typeof(ApiResponse<RequestDetailsDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    [ProducesResponseType(typeof(ApiResponse), 409)]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeRequestStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildValidationError("Validation failed"));

        if (!await _requestService.RequestExistsAsync(id))
            return NotFound(ApiResponse.ErrorResponse($"Request with ID {id} not found"));

        var (userId, userRole) = GetUserContext();
        var result = await _requestService.ChangeStatusAsync(id, dto, userId, userRole);

        if (result == null)
            return Conflict(ApiResponse.ErrorResponse(
                "Status transition not allowed or you don't have permission for this request"));

        return Ok(ApiResponse<RequestDetailsDto>.SuccessResponse(result, "Status updated successfully"));
    }

    /// <summary>Get comments for a request</summary>
    [HttpGet("{id:int}/comments")]
    [ProducesResponseType(typeof(ApiResponse<List<CommentDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetComments(int id)
    {
        if (!await _requestService.RequestExistsAsync(id))
            return NotFound(ApiResponse.ErrorResponse($"Request with ID {id} not found"));

        var (userId, userRole) = GetUserContext();
        var comments = await _requestService.GetCommentsAsync(id, userId, userRole);
        return Ok(ApiResponse<List<CommentDto>>.SuccessResponse(comments, "Comments retrieved successfully"));
    }

    /// <summary>Add a comment to a request</summary>
    [HttpPost("{id:int}/comments")]
    [ProducesResponseType(typeof(ApiResponse<CommentDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse), 400)]
    [ProducesResponseType(typeof(ApiResponse), 403)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> AddComment(int id, [FromBody] AddCommentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildValidationError("Validation failed"));

        if (!await _requestService.RequestExistsAsync(id))
            return NotFound(ApiResponse.ErrorResponse($"Request with ID {id} not found"));

        var (userId, userRole) = GetUserContext();
        var comment = await _requestService.AddCommentAsync(id, dto, userId, userRole);

        if (comment == null)
            return StatusCode(403, ApiResponse.ErrorResponse("You don't have access to this request"));

        return StatusCode(201, ApiResponse<CommentDto>.SuccessResponse(comment, "Comment added successfully"));
    }

    /// <summary>Get history/timeline for a request</summary>
    [HttpGet("{id:int}/history")]
    [ProducesResponseType(typeof(ApiResponse<List<HistoryDto>>), 200)]
    [ProducesResponseType(typeof(ApiResponse), 404)]
    public async Task<IActionResult> GetHistory(int id)
    {
        if (!await _requestService.RequestExistsAsync(id))
            return NotFound(ApiResponse.ErrorResponse($"Request with ID {id} not found"));

        var (userId, userRole) = GetUserContext();
        var history = await _requestService.GetHistoryAsync(id, userId, userRole);
        return Ok(ApiResponse<List<HistoryDto>>.SuccessResponse(history, "History retrieved successfully"));
    }

    // ── Private helpers ──────────────────────────────────────────────────

    private string GetUserId() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    private string GetUserRole() =>
        User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    private (string userId, string userRole) GetUserContext() =>
        (GetUserId(), GetUserRole());

    private ApiResponse BuildValidationError(string message)
    {
        var errors = ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(e => new ValidationError
            {
                Field = x.Key,
                Message = e.ErrorMessage
            }))
            .ToList();
        return ApiResponse.ErrorResponse(message, errors);
    }
}
