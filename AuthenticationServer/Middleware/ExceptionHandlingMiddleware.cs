using System.Net;
using System.Text.Json;
using AuthenticationServer.Exceptions;

namespace AuthenticationServer.Middleware;

public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception occurred while processing request. " +
                "Method: {Method}, Path: {Path}",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, message) = exception switch
        {
            InvalidCredentialsException =>
                (HttpStatusCode.Unauthorized,
                 "Invalid email or password."),

            InvalidRefreshTokenException =>
                (HttpStatusCode.Unauthorized,
                 "Invalid refresh token."),

            InvalidPasswordResetTokenException =>
                (HttpStatusCode.BadRequest,
                 "Invalid password reset token."),

            InvalidEmailVerificationTokenException =>
                (HttpStatusCode.BadRequest,
                 "Invalid email verification token."),

            EmailAlreadyExistsException =>
                (HttpStatusCode.Conflict,
                 exception.Message),

            _ =>
                (HttpStatusCode.InternalServerError,
                 "An unexpected error occurred.")
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new
        {
            statusCode = (int)statusCode,
            message
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}