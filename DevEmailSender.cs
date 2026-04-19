using Microsoft.AspNetCore.Identity.UI.Services;

namespace SuperFit.Services
{
    // DEV sender: не праща истински имейл, а пази последното "изпратено" съобщение
    public class DevEmailSender : IEmailSender
    {
        public static string? LastTo { get; private set; }
        public static string? LastSubject { get; private set; }
        public static string? LastHtmlMessage { get; private set; }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            LastTo = email;
            LastSubject = subject;
            LastHtmlMessage = htmlMessage;

            // може и Console.WriteLine за удобство
            Console.WriteLine("=== DEV EMAIL ===");
            Console.WriteLine($"TO: {email}");
            Console.WriteLine($"SUBJECT: {subject}");
            Console.WriteLine(htmlMessage);
            Console.WriteLine("=================");

            return Task.CompletedTask;
        }
    }
}
