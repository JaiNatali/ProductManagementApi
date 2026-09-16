using System.Net;
using System.Text.Json;

namespace API.Middleware;

public sealed class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? string.Empty;
            _logger.LogError(ex, "Unhandled exception. Method: {Method} Path: {Path} TraceIdentifier: {TraceIdentifier} ExceptionType: {ExceptionType}", method, path, context.TraceIdentifier, ex.GetType().FullName);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = new { message = "An unexpected error occurred." };
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
