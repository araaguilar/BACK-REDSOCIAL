using RedSocial.Domain.Entities;

namespace RedSocial.Application.Interfaces.Persistence;

public interface IEmailVerificationRepository
{
    Task<VerificacionEmail?> ObtenerActivaPorEmailAsync(string email, CancellationToken ct = default);
    Task<VerificacionEmail?> ObtenerVerificadaPorTokenAsync(string token, CancellationToken ct = default);
    Task InvalidarPendientesAsync(string email, CancellationToken ct = default);
    Task EliminarPendientesAsync(string email, CancellationToken ct = default);
    Task AgregarAsync(VerificacionEmail verificacion, CancellationToken ct = default);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
