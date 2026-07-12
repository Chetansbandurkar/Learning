using System;
using System.Threading;

class Program
{
    static void Work()
    {
        Console.WriteLine($"Running on Thread {Thread.CurrentThread.ManagedThreadId}");
    }

    static void Main(string[] args)
    {
        Thread thread = new Thread(Work);

        thread.Start();

        //thread.Join();
        thread.Join();

        //Thread t1 = new Thread(()-> {
        //    // Some long-running task
        //});
        //t1.start();

        //// The main thread pauses here until t1 finishes
        //t1.join();
        Console.WriteLine("Main Finished");
    }
}