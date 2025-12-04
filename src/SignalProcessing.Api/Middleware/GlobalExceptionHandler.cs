using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace SignalProcessing.Api.Middleware;

/// <summary>
/// Global exception handler for consistent error responses.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "An error occurred: {Message}",
            exception.Message
        );

        var (statusCode, message) = exception switch
        {
            InvalidOperationException => (StatusCodes.Status400BadRequest, exception.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized access"),
            _ => (StatusCodes.Status500InternalServerError, "An internal error occurred")
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(new
        {
            error = message,
            statusCode,
            timestamp = DateTime.UtcNow
        }, cancellationToken);

        return true;
    }
}
