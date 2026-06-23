using AlumniManagementSystem.Common;
using System.Net;
using System.Text.Json;

namespace AlumniManagementSystem.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteErrorResponse(context, ex);
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var env = context.RequestServices.GetService<IWebHostEnvironment>();
        bool isDev = env?.IsDevelopment() ?? false;

        (int statusCode, string message) = ex switch
        {
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, ex.Message),
            KeyNotFoundException        => (StatusCodes.Status404NotFound,     ex.Message),
            ArgumentException           => (StatusCodes.Status400BadRequest,   ex.Message),
            InvalidOperationException   => (StatusCodes.Status400BadRequest,   ex.Message),
            _                           => (StatusCodes.Status500InternalServerError,
                                            isDev ? $"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}" : "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;

        var response = ApiResponse.Fail(message);
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
