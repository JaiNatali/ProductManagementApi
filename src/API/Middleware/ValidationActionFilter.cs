using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Middleware;

public sealed class ValidationActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        var failures = context.ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .SelectMany(kvp => kvp.Value!.Errors.Select(error => new ValidationFailure(kvp.Key, error.ErrorMessage ?? "Invalid value.")))
            .ToList();

        throw new ValidationException(failures);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
