using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Perfil;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Application.Interfaces.Services;

namespace RedSocial.Application.Services;

public class PerfilService : IPerfilService
{
    private readonly IPerfilRepository _perfiles;

    public PerfilService(IPerfilRepository perfiles) => _perfiles = perfiles;

    public async Task<Resultado<MiPerfilDto>> ObtenerMiPerfilAsync(int idUsuario, CancellationToken ct = default)
    {
        var perfil = await _perfiles.ObtenerMiPerfilAsync(idUsuario, ct);
        return perfil is null
            ? Resultado<MiPerfilDto>.Error("Perfil no encontrado")
            : Resultado<MiPerfilDto>.Ok(perfil);
    }
}
