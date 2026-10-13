using ECommerce.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ECommerce.API.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "An exception occurred: {Message}",
            exception.Message);

        if (exception is ConcurrencyException)
        {
            httpContext.Response.StatusCode =
                StatusCodes.Status409Conflict;

            await httpContext.Response.WriteAsJsonAsync(
                new
                {
                    title = "Concurrency conflict",
                    status = 409,
                    detail = exception.Message
                },
                cancellationToken);

            return true;
        }

        return false;
    }
}