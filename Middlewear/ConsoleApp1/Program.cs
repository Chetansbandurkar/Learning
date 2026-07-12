namespace MiddlewarePipelineDemo;

class Program
{
    static async Task Main(string[] args)
    {
        var context = new HttpContext();

        // Endpoint
        RequestDelegate endpoint = Controller.HandleRequest;

        // Build pipeline manually
        var middlewareC = new MiddlewareC(endpoint);

        var middlewareB = new MiddlewareB(middlewareC.Invoke);

        var middlewareA = new MiddlewareA(middlewareB.Invoke);

        Console.WriteLine("========== REQUEST START ==========\n");

        await middlewareA.Invoke(context);

        Console.WriteLine("\n========== REQUEST END ==========");
    }
}