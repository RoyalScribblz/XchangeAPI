using System.Net;
using System.Text.Json;
using FluentValidation;

namespace XchangeAPI.Middleware;

/// <summary>
/// Catch exceptions thrown by fluent assertions and put them in the response body.
/// </summary>
/// <param name="next">The next delegate in the pipeline.</param>
public sealed class ValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) HttpStatusCode.BadRequest;

            ValidationErrorResponse errorResponse = new(false, ex.Errors.Select(
                e => new ValidationError(e.PropertyName, e.ErrorMessage)));
            
            string json = JsonSerializer.Serialize(errorResponse);
            await context.Response.WriteAsync(json);
        }
    }
}

internal sealed record ValidationErrorResponse(bool Success, IEnumerable<ValidationError> Errors);

internal sealed record ValidationError(string PropertyName, string ErrorMessage);