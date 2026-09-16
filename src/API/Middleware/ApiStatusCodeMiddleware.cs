using System.Net;
using System.Text.Json;
using API.Models;

namespace API.Middleware;

public sealed class ApiStatusCodeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiStatusCodeMiddleware> _logger;

    public ApiStatusCodeMiddleware(RequestDelegate next, ILogger<ApiStatusCodeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.Response.HasStarted)
        {
            return;
        }

        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized || context.Response.StatusCode == StatusCodes.Status403Forbidden)
        {
            _logger.LogWarning("Authorization failure detected. StatusCode: {StatusCode} Path: {Path} TraceId: {TraceId}", context.Response.StatusCode, context.Request.Path, context.TraceIdentifier);

            context.Response.ContentType = "application/json";

            var error = new ErrorResponse
            {
                Success = false,
                StatusCode = context.Response.StatusCode,
                Message = context.Response.StatusCode == StatusCodes.Status401Unauthorized ? "Unauthorized." : "Forbidden.",
                TraceId = context.TraceIdentifier,
                Timestamp = DateTimeOffset.UtcNow
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(error, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        }
    }
}
