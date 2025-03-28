namespace EmailService.Core.Models
{
    /// <summary>
    /// Standard response model for all API endpoints
    /// </summary>
    /// <typeparam name="T">The type of data returned by the API</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Human-readable message (generally used for errors)
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The data payload returned by the API
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Error details, if any
        /// </summary>
        public object? Errors { get; set; }

        /// <summary>
        /// Create a successful response
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Create an error response
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, object? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}