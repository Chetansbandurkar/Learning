namespace MiddlewarePipelineDemo;

public class MiddlewareB
{
    private readonly RequestDelegate _next;

    public MiddlewareB(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        Console.WriteLine("B Before");

        Console.WriteLine($"Time : {DateTime.Now:HH:mm:ss.fff}");

        await _next(context);

        Console.WriteLine($"Time : {DateTime.Now:HH:mm:ss.fff}");

        Console.WriteLine("B After");
    }
}