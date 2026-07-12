namespace MiddlewarePipelineDemo;

public class MiddlewareC
{
    private readonly RequestDelegate _next;

    public MiddlewareC(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        Console.WriteLine("C Before");

        Console.WriteLine($"Time : {DateTime.Now:HH:mm:ss.fff}");

        await _next(context);

        Console.WriteLine($"Time : {DateTime.Now:HH:mm:ss.fff}");

        Console.WriteLine("C After");
    }
}