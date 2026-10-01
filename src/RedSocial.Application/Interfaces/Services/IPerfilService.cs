using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Perfil;

namespace RedSocial.Application.Interfaces.Services;

public interface IPerfilService
{
    Task<Resultado<MiPerfilDto>> ObtenerMiPerfilAsync(int idUsuario, CancellationToken ct = default);
    Task<Resultado<MiPerfilDto>> ActualizarSobreMiAsync(int idUsuario, string? sobreMi, CancellationToken ct = default);
}
