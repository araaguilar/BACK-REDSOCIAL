namespace RedSocial.Application.Interfaces.Services;

public interface IEmailSender
{
    Task EnviarCodigoVerificacionAsync(string email, string codigo, CancellationToken ct = default);
    Task EnviarAvisoPasswordCambiadaAsync(string email, CancellationToken ct = default);
}
