using System;
using System.IO;
using MailKit.Net.Smtp;
using MailKit.Security;
using MeePoint.Helpers;
using MeePoint.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace MeePoint.Services
{
	public class EmailService : IEmailService
	{
		private readonly AppSettings _appSettings;
		private readonly IHostEnvironment _he;

		public EmailService(IOptions<AppSettings> appSettings, IHostEnvironment he)
		{
			_appSettings = appSettings.Value;
			_he = he;
		}

		public void Send(string from, string to, string subject, string html)
		{
			// create message
			var email = new MimeMessage();
			email.From.Add(MailboxAddress.Parse(from));
			email.To.Add(MailboxAddress.Parse(to));
			email.Subject = subject;
			email.Body = new TextPart(TextFormat.Html) { Text = html };

			// send email
			using var smtp = new SmtpClient();
			smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
			smtp.Authenticate(_appSettings.SmtpUser, _appSettings.SmtpPass);
			smtp.Send(email);
			smtp.Disconnect(true);
		}

		public void SendAccountCreated(string from, string to, string entity, string accountPassword)
		{
			// create message
			var email = new MimeMessage();
			email.From.Add(MailboxAddress.Parse(from));
			email.To.Add(MailboxAddress.Parse(to));
			email.Subject = $"MeePoint: A {entity} está a convidar a si para integrar na equipa de membros";

			var builder = new BodyBuilder();
			using (StreamReader SourceReader = System.IO.File.OpenText(Path.Combine(_he.ContentRootPath, "wwwroot/email/index.html")))
			{
				builder.HtmlBody = SourceReader.ReadToEnd();
			}
			builder.HtmlBody = builder.HtmlBody.Replace("{nome da organizacao}", entity);
			builder.HtmlBody = builder.HtmlBody.Replace("{Email}", to);
			builder.HtmlBody = builder.HtmlBody.Replace("{Password}", accountPassword);

			email.Body = builder.ToMessageBody();

			// Enviar o email
			using var smtp = new SmtpClient();
			smtp.Connect(_appSettings.SmtpHost, _appSettings.SmtpPort, SecureSocketOptions.StartTls);
			smtp.Authenticate(_appSettings.SmtpUser, _appSettings.SmtpPass);
			smtp.Send(email);
			smtp.Disconnect(true);
		}
	}
}