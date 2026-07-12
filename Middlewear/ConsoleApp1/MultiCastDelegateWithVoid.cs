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
        Console.WriteLine("SMS Sent");
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
        notify();
    }
}