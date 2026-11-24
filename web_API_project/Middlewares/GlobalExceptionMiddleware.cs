using System.Text.Json;
using web_API_project.Common;

namespace web_API_project.Middlewares
{
    public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unhandled API exception for {Path}", context.Request.Path);
                context.Response.StatusCode = exception is InvalidOperationException
                    ? StatusCodes.Status400BadRequest
                    : StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                var response = ApiResponse<object>.FailResponse(
                    context.Response.StatusCode == StatusCodes.Status400BadRequest ? exception.Message : "An unexpected error occurred.",
                    context.Response.StatusCode);
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}
