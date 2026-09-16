using System.Net;
using System.Text.Json;
using API.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Middleware;

public sealed class ApiExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionMiddleware> _logger;

    public ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
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
            LogException(ex);
            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static HttpStatusCode MapExceptionToStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            InvalidOperationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError,
        };
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception exception)
    {
        var status = MapExceptionToStatusCode(exception);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var error = new ErrorResponse
        {
            Success = false,
            StatusCode = context.Response.StatusCode,
            Message = GetErrorMessage(exception),
            TraceId = context.TraceIdentifier,
            Timestamp = DateTimeOffset.UtcNow
        };

        if (exception is ValidationException validationException)
        {
            error.Errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(error, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }

    private void LogException(Exception exception)
    {
        if (exception is ValidationException || exception is KeyNotFoundException || exception is UnauthorizedAccessException || exception is InvalidOperationException)
        {
            _logger.LogWarning(exception, "Client error handled by exception middleware.");
            return;
        }

        _logger.LogError(exception, "Unhandled exception handled by exception middleware.");
    }

    private static string GetErrorMessage(Exception exception)
        => exception switch
        {
            ValidationException => "One or more validation errors occurred.",
            KeyNotFoundException => exception.Message,
            UnauthorizedAccessException => "Unauthorized.",
            InvalidOperationException => exception.Message,
            _ => "An unexpected error occurred."
        };
}
