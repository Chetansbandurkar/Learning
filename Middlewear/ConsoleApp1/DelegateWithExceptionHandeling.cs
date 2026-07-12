using System;
public delegate void MyDelegate();
public delegate void Notify();

class Program
{
    static void SendEmail()
    {
        Console.WriteLine("Email Sent");
    }

    static void SendSMS()
    {
        throw new Exception("SMS Failed");
    }

    static void SendPush()
    {
        Console.WriteLine("Push Notification Sent");
    }
    static void Main()
    {
        Notify notify = SendEmail;
        notify += SendSMS;
        notify += SendPush;


        foreach (Notify handler in notify.GetInvocationList())
        {
            try
            {
                handler();
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
            }
        }



    }
}