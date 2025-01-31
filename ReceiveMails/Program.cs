using System;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Xml;

class Program
{
    static async Task Main(string[] args)
    {
        string imapHost = "imap.gmail.com";
        int imapPort = 993;
        string emailAddress = "your-email@gmail.com";
        string emailPassword = "your-app-password";

        var imapService = new ImapEmailService(imapHost, imapPort, emailAddress, emailPassword);

        Console.WriteLine("Starting email listener...");
        Console.WriteLine("Press 'q' to stop the listener.");

        _ = Task.Run(() => imapService.StartListeningAsync());

        while (Console.ReadKey(true).Key != ConsoleKey.Q) { }

        await imapService.StopListeningAsync();

        Console.WriteLine("Email listener stopped.");
    }
}


