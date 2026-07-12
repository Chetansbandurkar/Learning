using System;
using System.Threading.Tasks;

class Program
{
    static async void Save()
    {
        Console.WriteLine("Saving started...");

        await Task.Delay(3000);

        Console.WriteLine("Saving completed.");
    }

    static void Main(string[] args)
    {
        await Save();

        Console.WriteLine("Main method completed.");

        Console.ReadLine();
    }
}