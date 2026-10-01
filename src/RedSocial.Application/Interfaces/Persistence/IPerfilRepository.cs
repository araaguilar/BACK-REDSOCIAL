using RedSocial.Application.DTOs.Perfil;

namespace RedSocial.Application.Interfaces.Persistence;

public interface IPerfilRepository
{
    Task<MiPerfilDto?> ObtenerMiPerfilAsync(int idUsuario, CancellationToken ct = default);
}
