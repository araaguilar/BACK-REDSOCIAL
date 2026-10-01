using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Perfil;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Application.Interfaces.Services;

namespace RedSocial.Application.Services;

public class PerfilService : IPerfilService
{
    private const string CambioNombrePerfil = "nombre_perfil";
    private const string CambioNombreUsuario = "nombre_usuario";
    private readonly IPerfilRepository _perfiles;
    private readonly IUsuarioRepository _usuarios;

    public PerfilService(IPerfilRepository perfiles, IUsuarioRepository usuarios)
    {
        _perfiles = perfiles;
        _usuarios = usuarios;
    }

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

    public async Task<Resultado<MiPerfilDto>> ActualizarFotoPerfilAsync(int idUsuario, string fotoPerfilUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fotoPerfilUrl) || fotoPerfilUrl.Length > 500)
            return Resultado<MiPerfilDto>.Error("La ruta de la foto no es valida.");

        var perfil = await _perfiles.ActualizarFotoPerfilAsync(idUsuario, fotoPerfilUrl, ct);
        return perfil is null
            ? Resultado<MiPerfilDto>.Error("Perfil no encontrado")
            : Resultado<MiPerfilDto>.Ok(perfil, "Foto de perfil actualizada.");
    }

    public async Task<Resultado<MiPerfilDto>> ActualizarNombrePerfilAsync(int idUsuario, string nombrePerfil, CancellationToken ct = default)
    {
        var nuevoNombre = nombrePerfil.Trim();
        if (nuevoNombre.Length is < 2 or > 60)
            return Resultado<MiPerfilDto>.Error("El nombre de perfil debe tener entre 2 y 60 caracteres.");

        var perfilActual = await _perfiles.ObtenerMiPerfilAsync(idUsuario, ct);
        if (perfilActual is null) return Resultado<MiPerfilDto>.Error("Perfil no encontrado");
        if (string.Equals(perfilActual.NombrePerfil, nuevoNombre, StringComparison.Ordinal))
            return Resultado<MiPerfilDto>.Ok(perfilActual);

        var ultimoCambio = await _perfiles.ObtenerUltimoCambioAsync(idUsuario, CambioNombrePerfil, ct);
        var proximoCambio = ultimoCambio?.AddDays(3);
        if (proximoCambio.HasValue && proximoCambio.Value > DateTime.UtcNow)
            return Resultado<MiPerfilDto>.Error($"Podras cambiar tu nombre de perfil el {proximoCambio.Value:dd/MM/yy}.");

        var perfil = await _perfiles.ActualizarNombrePerfilAsync(idUsuario, nuevoNombre, ct);
        return perfil is null
            ? Resultado<MiPerfilDto>.Error("Perfil no encontrado")
            : Resultado<MiPerfilDto>.Ok(perfil, "Nombre de perfil actualizado.");
    }

    public async Task<Resultado<MiPerfilDto>> ActualizarNombreUsuarioAsync(int idUsuario, string nombreUsuario, CancellationToken ct = default)
    {
        var nuevoUsuario = nombreUsuario.Trim().TrimStart('@').ToLowerInvariant();
        if (nuevoUsuario.Length is < 3 or > 30)
            return Resultado<MiPerfilDto>.Error("El usuario debe tener entre 3 y 30 caracteres.");

        if (!nuevoUsuario.Contains('_') || !nuevoUsuario.All(c => char.IsLetterOrDigit(c) || c == '_'))
            return Resultado<MiPerfilDto>.Error("El usuario debe incluir _ y usar solo letras, numeros o guion bajo.");

        var perfilActual = await _perfiles.ObtenerMiPerfilAsync(idUsuario, ct);
        if (perfilActual is null) return Resultado<MiPerfilDto>.Error("Perfil no encontrado");
        if (string.Equals(perfilActual.NombreUsuario, nuevoUsuario, StringComparison.Ordinal))
            return Resultado<MiPerfilDto>.Ok(perfilActual);

        if (await _usuarios.ExisteNombreUsuarioAsync(nuevoUsuario, ct))
            return Resultado<MiPerfilDto>.Error("Ese usuario no esta disponible.");

        var ultimoCambio = await _perfiles.ObtenerUltimoCambioAsync(idUsuario, CambioNombreUsuario, ct);
        var proximoCambio = ultimoCambio?.AddDays(21);
        if (proximoCambio.HasValue && proximoCambio.Value > DateTime.UtcNow)
            return Resultado<MiPerfilDto>.Error($"Podras cambiar tu usuario el {proximoCambio.Value:dd/MM/yy}.");

        var perfil = await _perfiles.ActualizarNombreUsuarioAsync(idUsuario, nuevoUsuario, ct);
        return perfil is null
            ? Resultado<MiPerfilDto>.Error("Perfil no encontrado")
            : Resultado<MiPerfilDto>.Ok(perfil, "Usuario actualizado.");
    }
}
