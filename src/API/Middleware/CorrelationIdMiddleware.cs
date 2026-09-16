using System.Diagnostics;
using Serilog.Context;

namespace API.Middleware;

public sealed class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationIdHeader))
        {
            correlationIdHeader = Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
        }

        var correlationId = correlationIdHeader.ToString();
        context.TraceIdentifier = correlationId;
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("TraceIdentifier", context.TraceIdentifier))
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    LogContext.PushProperty("UserId", userId);
                }
            }

            await _next(context);
        }
    }
}
