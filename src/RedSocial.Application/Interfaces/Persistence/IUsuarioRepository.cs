using RedSocial.Domain.Entities;

namespace RedSocial.Application.Interfaces.Persistence;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorUsuarioOEmailAsync(string usuarioOEmail, CancellationToken ct = default);
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken ct = default);
    Task<bool> ExisteNombreUsuarioEnLookupAsync(string nombreUsuario, CancellationToken ct = default);
    Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default);
    Task AgregarAsync(Usuario usuario, CancellationToken ct = default);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
