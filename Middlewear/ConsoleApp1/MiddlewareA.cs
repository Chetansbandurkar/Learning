namespace MiddlewarePipelineDemo;

public class MiddlewareA(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        Console.WriteLine("A Before");

        Console.WriteLine($"Time : {DateTime.Now:HH:mm:ss.fff}");

        await _next(context);

        Console.WriteLine($"Time : {DateTime.Now:HH:mm:ss.fff}");

        Console.WriteLine("A After");
    }
}