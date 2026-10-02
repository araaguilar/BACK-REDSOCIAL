using RedSocial.Application.DTOs.Perfil;

namespace RedSocial.Application.Interfaces.Persistence;

public interface IPerfilRepository
{
    Task<MiPerfilDto?> ObtenerMiPerfilAsync(int idUsuario, CancellationToken ct = default);
    Task<PerfilPublicoDto?> ObtenerPerfilPublicoAsync(string nombreUsuario, int idUsuarioActual, CancellationToken ct = default);
    Task<SeguimientoPerfilDto?> AlternarSeguimientoAsync(int idSeguidor, int idSeguido, CancellationToken ct = default);
    Task<MiPerfilDto?> ActualizarSobreMiAsync(int idUsuario, string? sobreMi, CancellationToken ct = default);
    Task<MiPerfilDto?> ActualizarFotoPerfilAsync(int idUsuario, string fotoPerfilUrl, CancellationToken ct = default);
    Task<DateTime?> ObtenerUltimoCambioAsync(int idUsuario, string tipoCambio, CancellationToken ct = default);
    Task<MiPerfilDto?> ActualizarNombrePerfilAsync(int idUsuario, string nombrePerfil, CancellationToken ct = default);
    Task<MiPerfilDto?> ActualizarNombreUsuarioAsync(int idUsuario, string nombreUsuario, CancellationToken ct = default);
}
