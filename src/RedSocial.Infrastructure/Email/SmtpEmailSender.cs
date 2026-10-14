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

        await EnviarAsync(email, "Tu código de verificación de Moment", $"Tu código de verificación es {codigo}. Expira en 10 minutos.", ct);
    }

    public async Task EnviarAvisoPasswordCambiadaAsync(string email, CancellationToken ct = default)
    {
        if (!_settings.Enabled || string.IsNullOrWhiteSpace(_settings.Host))
        {
            _logger.LogWarning("Aviso de cambio de contraseña para {Email}", email);
            return;
        }

        await EnviarAsync(
            email,
            "Tu contraseña de Moment fue actualizada",
            "Tu contraseña de Moment acaba de cambiar. Si no fuiste tú, recupera tu cuenta cuanto antes.",
            ct);
    }

    private async Task EnviarAsync(string email, string asunto, string texto, CancellationToken ct)
    {
        var mensaje = new MimeMessage();
        mensaje.From.Add(MailboxAddress.Parse(_settings.From));
        mensaje.To.Add(MailboxAddress.Parse(email));
        mensaje.Subject = asunto;
        mensaje.Body = new TextPart("plain")
        {
            Text = texto
        };

        using var smtp = new SmtpClient();
        smtp.Timeout = 20000;
        var seguridad = _settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(20));

        await smtp.ConnectAsync(_settings.Host, _settings.Port, seguridad, timeout.Token);
        await smtp.AuthenticateAsync(_settings.User, _settings.Password, timeout.Token);
        await smtp.SendAsync(mensaje, timeout.Token);
        await smtp.DisconnectAsync(true, timeout.Token);
    }
}
