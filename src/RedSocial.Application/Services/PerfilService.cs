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

    public async Task<Resultado<MiPerfilDto>> ActualizarSobreMiAsync(int idUsuario, string? sobreMi, CancellationToken ct = default)
    {
        var texto = string.IsNullOrWhiteSpace(sobreMi) ? null : sobreMi.Trim();
        if (texto?.Length > 300)
            return Resultado<MiPerfilDto>.Error("El sobre mi no puede superar los 300 caracteres.");

        var perfil = await _perfiles.ActualizarSobreMiAsync(idUsuario, texto, ct);
        return perfil is null
            ? Resultado<MiPerfilDto>.Error("Perfil no encontrado")
            : Resultado<MiPerfilDto>.Ok(perfil, "Perfil actualizado.");
    }
}
