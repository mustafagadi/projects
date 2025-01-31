using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;

public class ImapEmailService
{
    private readonly string _imapHost;
    private readonly int _imapPort;
    private readonly string _emailAddress;
    private readonly string _emailPassword;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ImapEmailService(string imapHost, int imapPort, string emailAddress, string emailPassword)
    {
        _imapHost = imapHost;
        _imapPort = imapPort;
        _emailAddress = emailAddress;
        _emailPassword = emailPassword;
    }

    public async Task StartListeningAsync()
    {
        using var client = new ImapClient();

        try
        {
            await client.ConnectAsync(_imapHost, _imapPort, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(_emailAddress, _emailPassword);

            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadWrite);

            Console.WriteLine("Connected and listening for new emails...");

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                // Search for unseen emails
                IList<UniqueId> uids = await inbox.SearchAsync(SearchQuery.NotSeen);

                if (uids.Count > 0)
                {
                    await FetchAndDisplayNewEmailsAsync(inbox, uids);
                }

                await Task.Delay(5000); // Poll every 5 seconds
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

    private async Task FetchAndDisplayNewEmailsAsync(IMailFolder inbox, IList<UniqueId> uids)
    {
        foreach (var uid in uids)
        {
            var message = await inbox.GetMessageAsync(uid);
            Console.WriteLine("New Email Received!");
            Console.WriteLine($"From: {message.From}");
            Console.WriteLine($"Subject: {message.Subject}");
            Console.WriteLine($"Body: {message.TextBody}");
            Console.WriteLine(new string('-', 50));

            // Mark message as read
            await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true);
        }
    }

    public async Task StopListeningAsync()
    {
        _cancellationTokenSource.Cancel();
        await Task.CompletedTask;
    }
}
