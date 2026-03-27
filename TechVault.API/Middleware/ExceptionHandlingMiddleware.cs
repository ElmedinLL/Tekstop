using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Stripe;
using TechVault.API.Errors;

namespace TechVault.API.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            if (ex is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
            {
                throw;
            }

            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception after response started; rethrowing.");
            throw;
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = MapException(exception);

        if (statusCode >= 500)
        {
            logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            logger.LogWarning(exception, "Request failed: {Message}", message);
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";

        var body = new ApiErrorResponse
        {
            StatusCode = statusCode,
            Message = message,
            Errors = errors
        };

        await context.Response.WriteAsJsonAsync(body, JsonOptions, context.RequestAborted);
    }

    private (int StatusCode, string Message, object? Errors) MapException(Exception exception)
    {
        switch (exception)
        {
            case ArgumentNullException e:
                return ((int)HttpStatusCode.BadRequest, e.Message, null);
            case ArgumentException e:
                return ((int)HttpStatusCode.BadRequest, e.Message, null);
            case KeyNotFoundException e:
                return ((int)HttpStatusCode.NotFound, e.Message, null);
            case UnauthorizedAccessException e:
                return ((int)HttpStatusCode.Unauthorized, e.Message, null);
            case InvalidOperationException e:
                return ((int)HttpStatusCode.BadRequest, e.Message, null);
            case DbUpdateException e:
                return ((int)HttpStatusCode.Conflict, "The request could not be completed due to a data conflict.", null);
            case StripeException e:
                return ((int)HttpStatusCode.BadRequest, string.IsNullOrWhiteSpace(e.Message) ? "Payment request failed." : e.Message, null);
            case OperationCanceledException:
                return ((int)HttpStatusCode.RequestTimeout, "The request was cancelled.", null);
        }

        var msg = environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred.";

        return ((int)HttpStatusCode.InternalServerError, msg, null);
    }
}
