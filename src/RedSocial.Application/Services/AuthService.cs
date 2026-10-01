using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Auth;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Application.Interfaces.Security;
using RedSocial.Application.Interfaces.Services;
using RedSocial.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace RedSocial.Application.Services;

public class AuthService : IAuthService
{
    // Mensaje genérico: no revelamos si el usuario existe (evita enumeración de usuarios).
    private const string CredencialesInvalidas = "Usuario o contraseña incorrectos";
    private const int CodigoExpiraEnMinutos = 10;
    private const int IntentosMaximosCodigo = 5;
    private const string CodigoInvalido = "Código inválido o expirado";

    private readonly IUsuarioRepository _usuarios;
    private readonly IEmailVerificationRepository _verificaciones;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtGenerator _jwt;
    private readonly IEmailSender _emailSender;

    public AuthService(
        IUsuarioRepository usuarios,
        IEmailVerificationRepository verificaciones,
        IPasswordHasher hasher,
        IJwtGenerator jwt,
        IEmailSender emailSender)
    {
        _usuarios = usuarios;
        _verificaciones = verificaciones;
        _hasher = hasher;
        _jwt = jwt;
        _emailSender = emailSender;
    }

    public async Task<Resultado<SolicitarCodigoEmailResponseDto>> SolicitarCodigoEmailAsync(SolicitarCodigoEmailRequestDto dto, CancellationToken ct = default)
    {
        var email = NormalizarEmail(dto.Email);

        if (await _usuarios.ExisteEmailAsync(email, ct))
            return Resultado<SolicitarCodigoEmailResponseDto>.Error("El email ya está registrado");

        await _verificaciones.InvalidarPendientesAsync(email, ct);

        var codigo = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var token = CrearTokenSeguro();

        var verificacion = new VerificacionEmail
        {
            Email = email,
            CodigoHash = CrearHashCodigo(email, codigo, token),
            TokenVerificacion = token,
            ExpiraEn = DateTime.UtcNow.AddMinutes(CodigoExpiraEnMinutos)
        };

        await _verificaciones.AgregarAsync(verificacion, ct);
        await _verificaciones.GuardarCambiosAsync(ct);
        await _emailSender.EnviarCodigoVerificacionAsync(email, codigo, ct);

        return Resultado<SolicitarCodigoEmailResponseDto>.Ok(new SolicitarCodigoEmailResponseDto
        {
            ExpiraEnMinutos = CodigoExpiraEnMinutos
        }, "Te enviamos un código de verificación.");
    }

    public async Task<Resultado<VerificarCodigoEmailResponseDto>> VerificarCodigoEmailAsync(VerificarCodigoEmailRequestDto dto, CancellationToken ct = default)
    {
        var email = NormalizarEmail(dto.Email);
        var verificacion = await _verificaciones.ObtenerActivaPorEmailAsync(email, ct);

        if (verificacion is null)
            return Resultado<VerificarCodigoEmailResponseDto>.Error(CodigoInvalido);

        if (verificacion.IntentosFallidos >= IntentosMaximosCodigo)
        {
            verificacion.Usado = true;
            await _verificaciones.GuardarCambiosAsync(ct);
            return Resultado<VerificarCodigoEmailResponseDto>.Error(CodigoInvalido);
        }

        var hashRecibido = CrearHashCodigo(email, dto.Codigo.Trim(), verificacion.TokenVerificacion);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(hashRecibido),
                Encoding.UTF8.GetBytes(verificacion.CodigoHash)))
        {
            verificacion.IntentosFallidos++;
            await _verificaciones.GuardarCambiosAsync(ct);
            return Resultado<VerificarCodigoEmailResponseDto>.Error(CodigoInvalido);
        }

        verificacion.FechaVerificacion = DateTime.UtcNow;
        await _verificaciones.GuardarCambiosAsync(ct);

        return Resultado<VerificarCodigoEmailResponseDto>.Ok(new VerificarCodigoEmailResponseDto
        {
            VerificationToken = verificacion.TokenVerificacion
        }, "Correo verificado correctamente.");
    }

    public async Task<Resultado<object>> CancelarCodigoEmailAsync(SolicitarCodigoEmailRequestDto dto, CancellationToken ct = default)
    {
        var email = NormalizarEmail(dto.Email);
        await _verificaciones.EliminarPendientesAsync(email, ct);
        await _verificaciones.GuardarCambiosAsync(ct);

        return Resultado<object>.Ok(new { }, "Verificación cancelada.");
    }

    public async Task<Resultado<DisponibilidadUsuarioResponseDto>> VerificarDisponibilidadUsuarioAsync(string nombreUsuario, CancellationToken ct = default)
    {
        var nombre = nombreUsuario.Trim();
        if (nombre.Length < 3)
            return Resultado<DisponibilidadUsuarioResponseDto>.Error("El usuario debe tener al menos 3 caracteres");

        if (!System.Text.RegularExpressions.Regex.IsMatch(nombre, @"^[a-zA-Z0-9_.]+$"))
            return Resultado<DisponibilidadUsuarioResponseDto>.Error("Solo letras, números, punto y guion bajo");

        if (!nombre.Contains('_'))
            return Resultado<DisponibilidadUsuarioResponseDto>.Error("El usuario debe incluir al menos un guion bajo");

        var existe = await _usuarios.ExisteNombreUsuarioEnLookupAsync(nombre, ct);
        return Resultado<DisponibilidadUsuarioResponseDto>.Ok(new DisponibilidadUsuarioResponseDto
        {
            NombreUsuario = nombre,
            Disponible = !existe
        }, existe ? "Usuario no disponible" : "Usuario disponible");
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
        var email = NormalizarEmail(dto.Email);
        var nombrePerfil = dto.NombrePerfil.Trim();

        if (await _usuarios.ExisteNombreUsuarioAsync(nombre, ct))
            return Resultado<RegistroResponseDto>.Error("El nombre de usuario ya está en uso");

        if (await _usuarios.ExisteEmailAsync(email, ct))
            return Resultado<RegistroResponseDto>.Error("El email ya está registrado");

        if (dto.FechaNacimiento is null || CalcularEdad(dto.FechaNacimiento.Value) < 12)
            return Resultado<RegistroResponseDto>.Error("Debes tener al menos 12 años");

        var verificacion = await _verificaciones.ObtenerVerificadaPorTokenAsync(dto.VerificationToken.Trim(), ct);
        if (verificacion is null || verificacion.Email != email)
            return Resultado<RegistroResponseDto>.Error("Verifica tu correo antes de crear la cuenta");

        var usuario = new Usuario
        {
            NombreUsuario = nombre,
            NombrePerfil = nombrePerfil,
            Email = email,
            FechaNacimiento = dto.FechaNacimiento.Value,
            PasswordHash = _hasher.Hash(dto.Password), // BCrypt genera y guarda el salt dentro del hash
            Rol = "usuario",
            EmailVerificado = true
        };

        await _usuarios.AgregarAsync(usuario, ct);
        verificacion.Usado = true;
        await _usuarios.GuardarCambiosAsync(ct);

        return Resultado<RegistroResponseDto>.Ok(new RegistroResponseDto
        {
            IdUsuario = usuario.IdUsuario,
            NombreUsuario = usuario.NombreUsuario,
            NombrePerfil = usuario.NombrePerfil,
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
            NombrePerfil = usuario.NombrePerfil,
            Email = usuario.Email,
            Rol = usuario.Rol,
            EmailVerificado = usuario.EmailVerificado,
            Token = token,
            ExpiraEn = expira
        };
    }

    private static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

    private static string CrearTokenSeguro() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    private static string CrearHashCodigo(string email, string codigo, string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{email}:{codigo}:{token}"));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static int CalcularEdad(DateOnly nacimiento)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var edad = hoy.Year - nacimiento.Year;
        if (nacimiento > hoy.AddYears(-edad)) edad--;
        return edad;
    }
}
