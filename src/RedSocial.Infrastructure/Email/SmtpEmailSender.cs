using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using RedSocial.Application.Interfaces.Services;

namespace RedSocial.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> options, ILogger<SmtpEmailSender> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task EnviarCodigoVerificacionAsync(string email, string codigo, CancellationToken ct = default)
    {
        if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning("Código de verificación para {Email}: {Codigo}", email, codigo);
            return;
        }

        var mensaje = new MimeMessage();
        mensaje.From.Add(MailboxAddress.Parse(_settings.From));
        mensaje.To.Add(MailboxAddress.Parse(email));
        mensaje.Subject = "Tu código de verificación de Moment";
        mensaje.Body = new TextPart("plain")
        {
            Text = $"Tu código de verificación es {codigo}. Expira en 10 minutos."
        };

        using var smtp = new SmtpClient();
        var seguridad = _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;

        await smtp.ConnectAsync(_settings.Host, _settings.Port, seguridad, ct);
        await smtp.AuthenticateAsync(_settings.User, _settings.Password, ct);
        await smtp.SendAsync(mensaje, ct);
        await smtp.DisconnectAsync(true, ct);
    }
}
