namespace DogQueueApi.Services
{
    public class ServiceResult<T>
    {
        public bool Success { get; init; }
        public int StatusCode { get; init; }
        public string? Message { get; init; }
        public string[]? Errors { get; init; }
        public T? Data { get; init; }

        public static ServiceResult<T> Ok(T? data, string? message = null) =>
            new() { Success = true, StatusCode = 200, Data = data, Message = message };

        public static ServiceResult<T> BadRequest(string message, string[]? errors = null) =>
            new() { Success = false, StatusCode = 400, Message = message, Errors = errors };

        public static ServiceResult<T> Unauthorized(string message = "Invalid token") =>
            new() { Success = false, StatusCode = 401, Message = message };

        public static ServiceResult<T> NotFound(string message = "Not found") =>
            new() { Success = false, StatusCode = 404, Message = message };

        public static ServiceResult<T> Forbid(string message = "Forbidden") =>
            new() { Success = false, StatusCode = 403, Message = message };
    }
}
