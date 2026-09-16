using API.Extensions;
using API.Middleware;
using Infrastructure.Data;
using Serilog;
using Serilog.Events;
using Serilog.Enrichers;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithProperty("ApplicationName", context.HostingEnvironment.ApplicationName)
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .Enrich.WithProperty("Version", typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown")
        .Enrich.WithProperty("ProcessId", System.Diagnostics.Process.GetCurrentProcess().Id)
        .Enrich.WithProperty("ThreadId", System.Threading.Thread.CurrentThread.ManagedThreadId)
        .Filter.ByExcluding(logEvent => logEvent.Exception is not null && logEvent.Level == LogEventLevel.Information);
});

builder.Services.AddApiLayer(builder.Configuration);
builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("UserId", httpContext.User.Identity?.IsAuthenticated == true
            ? httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            : null);
        diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
    };
});
app.UseMiddleware<ApiExceptionMiddleware>();

var enableSwagger = app.Configuration.GetValue("EnableSwagger", true);
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
        options.ConfigObject.AdditionalItems["persistAuthorization"] = true;
    });
}

var enableHttpsRedirection = app.Configuration.GetValue("UseHttpsRedirection", !app.Environment.IsDevelopment());
if (enableHttpsRedirection)
{
    app.UseHttpsRedirection();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (app.Environment.IsEnvironment("Testing"))
{
    app.MapGet("/api/v1/test/error", (HttpContext context) => throw new Exception("Test exception"))
        .AllowAnonymous();
}

app.UseRouting();
app.UseCors("DefaultPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ApiStatusCodeMiddleware>();
app.MapControllers();

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
logger.LogInformation("Application starting. Environment: {Environment} ApplicationName: {ApplicationName} Version: {Version}", app.Environment.EnvironmentName, app.Environment.ApplicationName, typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown");

var skipDbInit = app.Environment.IsEnvironment("Testing") || app.Configuration.GetValue<bool>("SkipDatabaseInitialization", false);
if (!skipDbInit)
{
    await app.Services.InitializeDatabaseAsync(app.Configuration);
}

logger.LogInformation("Application started. Environment: {Environment} ApplicationName: {ApplicationName} Version: {Version}", app.Environment.EnvironmentName, app.Environment.ApplicationName, typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown");

app.Lifetime.ApplicationStopping.Register(() => logger.LogInformation("Application stopping. Environment: {Environment} ApplicationName: {ApplicationName}", app.Environment.EnvironmentName, app.Environment.ApplicationName));
app.Lifetime.ApplicationStopped.Register(() => logger.LogInformation("Application stopped. Environment: {Environment} ApplicationName: {ApplicationName}", app.Environment.EnvironmentName, app.Environment.ApplicationName));

app.Run();

public partial class Program { }
