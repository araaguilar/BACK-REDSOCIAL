using RedSocial.Domain.Entities;

namespace RedSocial.Application.Interfaces.Persistence;

public interface IPerfilUsuarioRepository
{
    Task AgregarAsync(PerfilUsuario perfil, CancellationToken ct = default);
}
