using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Perfil;

namespace RedSocial.Application.Interfaces.Services;

public interface IPerfilService
{
    Task<Resultado<MiPerfilDto>> ObtenerMiPerfilAsync(int idUsuario, CancellationToken ct = default);
    Task<Resultado<MiPerfilDto>> ActualizarSobreMiAsync(int idUsuario, string? sobreMi, CancellationToken ct = default);
    Task<Resultado<MiPerfilDto>> ActualizarFotoPerfilAsync(int idUsuario, string fotoPerfilUrl, CancellationToken ct = default);
    Task<Resultado<MiPerfilDto>> ActualizarNombrePerfilAsync(int idUsuario, string nombrePerfil, CancellationToken ct = default);
    Task<Resultado<MiPerfilDto>> ActualizarNombreUsuarioAsync(int idUsuario, string nombreUsuario, CancellationToken ct = default);
}
