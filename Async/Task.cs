using System;
using System.Threading.Tasks;

class Program
{
    static async Task Work()
    {
        Console.WriteLine($"Thread {Environment.CurrentManagedThreadId}");// thread 2

        await Task.Delay(2000);

        Console.WriteLine($"Thread {Environment.CurrentManagedThreadId}");// thread 5
    }

    static async Task Main(string[] args)
    {
        await Work();
    }

}