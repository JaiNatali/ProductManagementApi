using Microsoft.Net.Http.Headers;

namespace API.Middleware;

public sealed class SecurityHeadersMiddleware
{
    private const string StrictContentSecurityPolicy = "default-src 'self'; frame-ancestors 'none'; base-uri 'self'; form-action 'self'; object-src 'none'";
    private const string DevelopmentSwaggerContentSecurityPolicy = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; frame-ancestors 'none'; base-uri 'self'; form-action 'self'; object-src 'none'";

    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers[HeaderNames.XContentTypeOptions] = "nosniff";
        context.Response.Headers[HeaderNames.XFrameOptions] = "DENY";
        context.Response.Headers["Referrer-Policy"] = "no-referrer";
        context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        context.Response.Headers[HeaderNames.ContentSecurityPolicy] = GetContentSecurityPolicy(context);
        await _next(context);
    }

    private string GetContentSecurityPolicy(HttpContext context)
    {
        return _environment.IsDevelopment() && context.Request.Path.StartsWithSegments("/swagger")
            ? DevelopmentSwaggerContentSecurityPolicy
            : StrictContentSecurityPolicy;
    }
}
