using AuthenticationServer.Exceptions;

namespace AuthenticationServer.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex);
        }
    }

    private static async Task HandleException(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        switch (ex)
        {
            case EmailAlreadyExistsException:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                break;

            case InvalidCredentialsException:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                break;

        }

        await context.Response.WriteAsJsonAsync(new
        {
            message = ex.Message + " Chetan"    
        });
    }
}