namespace AlumniManagementSystem.Common;

/// <summary>Generic API response wrapper — all endpoints return this shape.</summary>
public class ApiResponse<T>
{
    public bool    Success { get; set; }
    public string  Message { get; set; } = string.Empty;
    public T?      Data    { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message) =>
        new() { Success = false, Message = message };
}

/// <summary>Non-generic variant for responses that carry no data payload.</summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string message = "Success") =>
        new() { Success = true, Message = message };

    public new static ApiResponse Fail(string message) =>
        new() { Success = false, Message = message };
}
