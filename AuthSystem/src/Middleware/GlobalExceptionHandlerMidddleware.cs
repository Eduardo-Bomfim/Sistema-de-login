using System.Net;
using AuthSystem.src.DTOs;

namespace AuthSystem.src.Middleware
{
    public class GlobalExceptionHandlerMidddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMidddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<GlobalExceptionHandlerMidddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new ErrorResponseDto
            {
                StatusCode = context.Response.StatusCode,
                Message = "An unexpected error occurred. Please try again later.",
                Details = exception.Message
            };

            if (context.Request.Host.Host.Contains("localhost"))
            {
                response.Details = exception.StackTrace?.ToString();
            }
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    }
}