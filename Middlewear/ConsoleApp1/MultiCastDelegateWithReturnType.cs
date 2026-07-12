using System;
public delegate void MyDelegate();
public delegate int Notify();

class Program
{
    static int SendEmail()
    {
        return 3;
        Console.WriteLine("Email Sent");
    }

    static int SendSMS()
    {
        return 2;
        Console.WriteLine("SMS Sent");
    }

    static int SendPush()
    {
        return 1;
        Console.WriteLine("Push Notification Sent");
    }
    static void Main()
    {
        Notify notify = SendEmail;

        notify += SendSMS;

        notify += SendPush;
        int val = notify();
        Console.WriteLine(val);

        // value will be of last method
    }
}