using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Auth;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Application.Interfaces.Security;
using RedSocial.Application.Interfaces.Services;
using RedSocial.Domain.Entities;

namespace RedSocial.Application.Services;

public class AuthService : IAuthService
{
    // Mensaje genérico: no revelamos si el usuario existe (evita enumeración de usuarios).
    private const string CredencialesInvalidas = "Usuario o contraseña incorrectos";

    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtGenerator _jwt;

    public AuthService(IUsuarioRepository usuarios, IPasswordHasher hasher, IJwtGenerator jwt)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<Resultado<AuthResponseDto>> LoginAsync(LoginRequestDto dto, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObtenerPorUsuarioOEmailAsync(dto.UsuarioOEmail.Trim(), ct);

        if (usuario is null)
        {
            // Si el usuario no existe igual hasheamos, para que el tiempo de respuesta
            // sea similar al de un usuario real (mitiga ataques de temporización).
            _hasher.Hash(dto.Password);
            return Resultado<AuthResponseDto>.Error(CredencialesInvalidas);
        }

        if (!usuario.Activo)
            return Resultado<AuthResponseDto>.Error("La cuenta está deshabilitada");

        // BCrypt toma el salt guardado dentro del hash y compara.
        if (!_hasher.Verificar(dto.Password, usuario.PasswordHash))
            return Resultado<AuthResponseDto>.Error(CredencialesInvalidas);

        usuario.UltimoLogin = DateTime.UtcNow;
        await _usuarios.GuardarCambiosAsync(ct);

        return Resultado<AuthResponseDto>.Ok(CrearRespuesta(usuario), $"Bienvenido, {usuario.NombreUsuario}");
    }

    public async Task<Resultado<RegistroResponseDto>> RegistrarAsync(RegistroRequestDto dto, CancellationToken ct = default)
    {
        var nombre = dto.NombreUsuario.Trim();
        var email = dto.Email.Trim().ToLowerInvariant();

        if (await _usuarios.ExisteNombreUsuarioAsync(nombre, ct))
            return Resultado<RegistroResponseDto>.Error("El nombre de usuario ya está en uso");

        if (await _usuarios.ExisteEmailAsync(email, ct))
            return Resultado<RegistroResponseDto>.Error("El email ya está registrado");

        var usuario = new Usuario
        {
            NombreUsuario = nombre,
            Email = email,
            PasswordHash = _hasher.Hash(dto.Password) // BCrypt genera y guarda el salt dentro del hash
        };

        await _usuarios.AgregarAsync(usuario, ct);
        await _usuarios.GuardarCambiosAsync(ct);

        return Resultado<RegistroResponseDto>.Ok(new RegistroResponseDto
        {
            IdUsuario = usuario.IdUsuario,
            NombreUsuario = usuario.NombreUsuario,
            Email = usuario.Email
        }, "Cuenta creada correctamente. Ya puedes iniciar sesión.");
    }

    private AuthResponseDto CrearRespuesta(Usuario usuario)
    {
        var (token, expira) = _jwt.Generar(usuario);
        return new AuthResponseDto
        {
            IdUsuario = usuario.IdUsuario,
            NombreUsuario = usuario.NombreUsuario,
            Email = usuario.Email,
            Token = token,
            ExpiraEn = expira
        };
    }
}
