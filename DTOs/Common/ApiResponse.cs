namespace MaintenanceRequestSystemAPI.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<ValidationError>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> ErrorResponse(string message, List<ValidationError>? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<ValidationError>? Errors { get; set; }

    public static ApiResponse SuccessResponse(string message = "Operation completed successfully")
        => new() { Success = true, Message = message };

    public static ApiResponse ErrorResponse(string message, List<ValidationError>? errors = null)
        => new() { Success = false, Message = message, Errors = errors };
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
