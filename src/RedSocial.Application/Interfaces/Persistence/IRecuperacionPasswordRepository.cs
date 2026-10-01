using RedSocial.Domain.Entities;

namespace RedSocial.Application.Interfaces.Persistence;

public interface IRecuperacionPasswordRepository
{
    Task<RecuperacionPassword?> ObtenerActivaPorEmailAsync(string email, CancellationToken ct = default);
    Task<RecuperacionPassword?> ObtenerVerificadaPorTokenAsync(string token, CancellationToken ct = default);
    Task InvalidarPendientesAsync(string email, CancellationToken ct = default);
    Task AgregarAsync(RecuperacionPassword recuperacion, CancellationToken ct = default);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
