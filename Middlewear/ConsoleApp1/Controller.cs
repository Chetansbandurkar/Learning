namespace MiddlewarePipelineDemo;

public static class Controller
{
    public static async Task HandleRequest(HttpContext context)
    {
        Console.WriteLine("Controller Started");

        await Task.Delay(1000);

        Console.WriteLine("Controller Finished");
    }
}