namespace Ethereal_api.IService;

public class Response
{
    /// <summary>
    /// 响应类
    /// </summary>
    /// <typeparam name="T">不定类型参数</typeparam>
    public abstract class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(bool success, int statusCode, T? data, string? message = null, List<string>? errors = null)
        {
            Success = success;
            StatusCode = statusCode;
            Data = data;
            Message = message;
            Errors = errors;
        }
    }

    /// <summary>
    /// 成功响应类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiSuccessResponse<T> : ApiResponse<T>
    {
        public ApiSuccessResponse(T data, int statusCode = 200, string? message = "Operation successful")
            : base(true, statusCode, data, message)
        {
        }
    }

    /// <summary>
    /// 错误响应类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiErrorResponse<T> : ApiResponse<T>
    {
        public ApiErrorResponse(string message, int statusCode = 400, List<string>? errors = null)
            : base(false, statusCode, default, message, errors)
        {
        }
    }
}