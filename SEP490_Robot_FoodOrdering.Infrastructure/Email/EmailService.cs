

using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Email;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Options;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;


namespace SEP490_Robot_FoodOrdering.Infrastructure.Email
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _emailOptions;
        private readonly IUtilsService _utils;

        public EmailService(IOptions<EmailOptions> emailOptions, IUtilsService utils)
        {
            _emailOptions = emailOptions.Value;
            _utils = utils;
        }
        public async Task SendPasswordResetEmailAsync(string to, string token)
        {
            var subject = "Cấp lại mật khẩu";
            string template = await _utils.GetEmailTemplateAsync("ResetPasswordpage.html", "EmailTemplates");
            string body = ReplaceTemplatePlaceholders(template, new Dictionary<string, string>
            {
                { "UserName", "Luongquoc Trieu" },
                { "1", token },
                {"Year" , DateTime.Now.Year.ToString() }
            });
            await SendEmailAsync(to, subject, body);
        }

        public async Task SendVerificationEmailAsync(string to, string token)
        {
            var subject = "Xác thực tài khoản";
            string template = await _utils.GetEmailTemplateAsync("VerifyAccountTemplate.html", "MailTemplates");
            string body = ReplaceTemplatePlaceholders(template, new Dictionary<string, string>
            {
                { "0", to },
                { "1", token }
            });
            await SendEmailAsync(to, subject, body);
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailOptions.SenderName,
                _emailOptions.SenderEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _emailOptions.SmtpServer,
                _emailOptions.SmtpPort, // Corrected parentheses
                SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailOptions.SmtpUsername, _emailOptions.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private string ReplaceTemplatePlaceholders(string template, Dictionary<string, string> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                return template;

            foreach (var param in parameters)
            {
                var placeholder = $"{{{param.Key}}}";
                template = template.Replace(placeholder, param.Value);
            }

            return template;
        }
    }
}
