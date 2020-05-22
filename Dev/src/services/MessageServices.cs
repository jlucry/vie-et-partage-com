using Contracts;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using MailKit.Security;

namespace Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link http://go.microsoft.com/fwlink/?LinkID=532713
    // Creating messages: http://www.mimekit.net/docs/html/Creating-Messages.htm
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        private readonly WcmsAppContext _Context = null;
        private readonly ILogger _logger;

        public AuthMessageSender(WcmsAppContext context)
        {
            _Context = context;
            _logger = context?.LoggerFactory?.CreateLogger<AuthMessageSender>();
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return SendEmailAsync(new List<string> { email }, subject, message);
        }

        public Task SendEmailAsync(List<string> emails, string subject, string message)
        {
            string emailString = null;
            try
            {
                // For debug...
                if (_logger != null && emails?.Count != 0)
                {
                    foreach (string eml in emails)
                        emailString += (eml + " ");
                }

                // Checking...
                if (emails == null || emails.Count == 0)
                {
                    _logger?.LogError(3, "Failed to send email {0}: No recipient.", subject);
                    return Task.FromResult(0);
                }
                else if (_Context?.Module?.Name == null)
                {
                    _logger?.LogError(3, "Failed to send email {0}: Invalid module name.", subject);
                    return Task.FromResult(0);
                }
                else if (string.IsNullOrEmpty(subject) == true
                    || string.IsNullOrEmpty(message) == true)
                {
                    _logger?.LogError(3, "Failed to send email to {0}: Invalid mail.", emailString);
                    return Task.FromResult(0);
                }

                string email_from_name = _Context.Configuration[$"{_Context.Module.Name}:email_from_name"];
                string email_from_address = _Context.Configuration[$"{_Context.Module.Name}:email_from_address"];
                string email_srv_address = _Context.Configuration[$"{_Context.Module.Name}:email_srv_address"];
                string email_srv_port = _Context.Configuration[$"{_Context.Module.Name}:email_srv_port"];
                string email_srv_ssl = _Context.Configuration[$"{_Context.Module.Name}:email_srv_ssl"];
                string email_srv_login = _Context.Configuration[$"{_Context.Module.Name}:email_srv_login"];
                string email_srv_password = _Context.Configuration[$"{_Context.Module.Name}:email_srv_password"];
                if (string.IsNullOrEmpty(email_from_name) == true
                    || string.IsNullOrEmpty(email_from_address) == true
                    || string.IsNullOrEmpty(email_srv_address) == true
                    || string.IsNullOrEmpty(email_srv_port) == true
                    || string.IsNullOrEmpty(email_srv_ssl) == true
                    || string.IsNullOrEmpty(email_srv_login) == true
                    || string.IsNullOrEmpty(email_srv_password) == true)
                {
                    _logger?.LogError(3, "Failed to send email {0} to {1}: Invalid email settings", subject, emailString);
                    return Task.FromResult(0);
                }

                // Send the email...
                _logger?.LogDebug(3, "Send email {0} to {1}.", subject, emailString);

                // https://github.com/jstedfast/MailKit
                var mimeMsg = new MimeMessage();
                mimeMsg.From.Add(new MailboxAddress(email_from_name, email_from_address));
                foreach (string eml in emails)
                {
                    if (string.IsNullOrEmpty(eml) == false)
                    {
                        mimeMsg.Cc/*To*/.Add(new MailboxAddress(eml));
                    }
                }
                mimeMsg.Subject = $"[{_Context.Module.LongName}]{subject}";
                mimeMsg.Body = new TextPart(TextFormat.Html)
                {
                    Text = message
                };

                using (var client = new SmtpClient())
                {
                    email_srv_ssl = email_srv_ssl.ToLower();
                    // None                     0   No SSL or TLS encryption should be used.
                    // Auto                     1   Allow the IMailService to decide which SSL or TLS options to use (default).
                    // SslOnConnect             2   The connection should use SSL or TLS encryption immediately.
                    // StartTls                 3   Elevates the connection to use TLS encryption immediately after reading the greeting and capabilities of the server.If the server does not support the STARTTLS extension, then the connection will fail and a NotSupportedException will be thrown.
                    // StartTlsWhenAvailable    4   Elevates the connection to use TLS encryption immediately after reading the greeting and capabilities of the server, but only if the server supports the STARTTLS extension.
                    SecureSocketOptions secOptions = SecureSocketOptions.StartTls;
                    if (email_srv_ssl == "true" || email_srv_ssl == "sslonconnect")
                        secOptions = SecureSocketOptions.SslOnConnect;
                    //else if (email_srv_ssl == "false")
                    //    secOptions = SecureSocketOptions.None;
                    //else if (email_srv_ssl == "starttls")
                    //    secOptions = SecureSocketOptions.StartTls;

                    // Accept all SSL certificates (in case the server supports STARTTLS)...
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    // Connect...
                    client.Connect(email_srv_address, int.Parse(email_srv_port), secOptions);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate(email_srv_login, email_srv_password);

                    client.Send(mimeMsg);
                    client.Disconnect(true);
                }

                return Task.FromResult(0);
            }
            catch (Exception e)
            {
                _logger?.LogError(3, "Failed to send email {0} to {1}: Exception: {2}", subject, emailString, e.Message);
                return Task.FromResult(0);
            }
        }

        public Task SendSmsAsync(string number, string message)
        {
            // Plug in your SMS service here to send a text message.
            return Task.FromResult(0);
        }
    }
}
