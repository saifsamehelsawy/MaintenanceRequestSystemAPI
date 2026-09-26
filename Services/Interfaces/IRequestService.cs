using MaintenanceRequestSystemAPI.DTOs.Common;
using MaintenanceRequestSystemAPI.DTOs.Requests;

namespace MaintenanceRequestSystemAPI.Services.Interfaces;

public interface IRequestService
{
    Task<PagedResult<RequestListDto>> GetRequestsAsync(RequestFilterDto filter, string userId, string userRole);
    Task<RequestDetailsDto?> GetRequestByIdAsync(int id, string userId, string userRole);
    Task<RequestDetailsDto> CreateRequestAsync(CreateRequestDto dto, string employeeId);
    Task<RequestDetailsDto?> UpdateRequestAsync(int id, UpdateRequestDto dto, string userId, string userRole);
    Task<bool> DeleteRequestAsync(int id, string userId, string userRole);
    Task<RequestDetailsDto?> AssignTechnicianAsync(int id, AssignTechnicianDto dto, string adminId);
    Task<RequestDetailsDto?> ChangeStatusAsync(int id, ChangeRequestStatusDto dto, string userId, string userRole);
    Task<List<CommentDto>> GetCommentsAsync(int requestId, string userId, string userRole);
    Task<CommentDto?> AddCommentAsync(int requestId, AddCommentDto dto, string userId, string userRole);
    Task<List<HistoryDto>> GetHistoryAsync(int requestId, string userId, string userRole);
    Task<bool> RequestExistsAsync(int id);
    Task<bool> CanAccessRequestAsync(int id, string userId, string userRole);
}
