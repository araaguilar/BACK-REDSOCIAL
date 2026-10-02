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
    private const int RegistroExpiraEnMinutos = 45;
    private const int RecuperacionExpiraEnMinutos = 20;
    private const int IntentosMaximosCodigo = 5;
    private const string CodigoInvalido = "Código inválido o expirado";

    private readonly IUsuarioRepository _usuarios;
    private readonly IPerfilUsuarioRepository _perfiles;
    private readonly IEmailVerificationRepository _verificaciones;
    private readonly IRecuperacionPasswordRepository _recuperacionesPassword;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtGenerator _jwt;
    private readonly IEmailSender _emailSender;

    public AuthService(
        IUsuarioRepository usuarios,
        IPerfilUsuarioRepository perfiles,
        IEmailVerificationRepository verificaciones,
        IRecuperacionPasswordRepository recuperacionesPassword,
        IPasswordHasher hasher,
        IJwtGenerator jwt,
        IEmailSender emailSender)
    {
        _usuarios = usuarios;
        _perfiles = perfiles;
        _verificaciones = verificaciones;
        _recuperacionesPassword = recuperacionesPassword;
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
        verificacion.ExpiraEn = DateTime.UtcNow.AddMinutes(RegistroExpiraEnMinutos);
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

    public async Task<Resultado<DisponibilidadEmailResponseDto>> VerificarDisponibilidadEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizado = NormalizarEmail(email);
        if (!System.Text.RegularExpressions.Regex.IsMatch(normalizado, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            return Resultado<DisponibilidadEmailResponseDto>.Error("Ingresa un correo válido");

        var existe = await _usuarios.ExisteEmailEnLookupAsync(normalizado, ct);
        return Resultado<DisponibilidadEmailResponseDto>.Ok(new DisponibilidadEmailResponseDto
        {
            Email = normalizado,
            Disponible = !existe
        }, existe ? "Correo no disponible" : "Correo disponible");
    }

    public async Task<Resultado<SolicitarCodigoEmailResponseDto>> SolicitarRecuperacionPasswordAsync(SolicitarRecuperacionPasswordRequestDto dto, CancellationToken ct = default)
    {
        var email = NormalizarEmail(dto.Email);
        var usuario = await _usuarios.ObtenerPorEmailAsync(email, ct);

        if (usuario is null || !usuario.Activo)
            return Resultado<SolicitarCodigoEmailResponseDto>.Error("No existe ninguna cuenta ligada a este correo");

        await _recuperacionesPassword.InvalidarPendientesAsync(email, ct);

        var codigo = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var token = CrearTokenSeguro();

        var recuperacion = new RecuperacionPassword
        {
            IdUsuario = usuario.IdUsuario,
            Email = email,
            CodigoHash = CrearHashCodigo(email, codigo, token),
            TokenRecuperacion = token,
            ExpiraEn = DateTime.UtcNow.AddMinutes(CodigoExpiraEnMinutos)
        };

        await _recuperacionesPassword.AgregarAsync(recuperacion, ct);
        await _recuperacionesPassword.GuardarCambiosAsync(ct);
        await _emailSender.EnviarCodigoVerificacionAsync(email, codigo, ct);

        return Resultado<SolicitarCodigoEmailResponseDto>.Ok(new SolicitarCodigoEmailResponseDto
        {
            ExpiraEnMinutos = CodigoExpiraEnMinutos
        }, "Si el correo existe, enviaremos un código de recuperación.");
    }

    public async Task<Resultado<VerificarRecuperacionPasswordResponseDto>> VerificarRecuperacionPasswordAsync(VerificarRecuperacionPasswordRequestDto dto, CancellationToken ct = default)
    {
        var email = NormalizarEmail(dto.Email);
        var recuperacion = await _recuperacionesPassword.ObtenerActivaPorEmailAsync(email, ct);

        if (recuperacion is null)
            return Resultado<VerificarRecuperacionPasswordResponseDto>.Error(CodigoInvalido);

        if (recuperacion.IntentosFallidos >= IntentosMaximosCodigo)
        {
            recuperacion.Usado = true;
            await _recuperacionesPassword.GuardarCambiosAsync(ct);
            return Resultado<VerificarRecuperacionPasswordResponseDto>.Error(CodigoInvalido);
        }

        var hashRecibido = CrearHashCodigo(email, dto.Codigo.Trim(), recuperacion.TokenRecuperacion);
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(hashRecibido),
                Encoding.UTF8.GetBytes(recuperacion.CodigoHash)))
        {
            recuperacion.IntentosFallidos++;
            await _recuperacionesPassword.GuardarCambiosAsync(ct);
            return Resultado<VerificarRecuperacionPasswordResponseDto>.Error(CodigoInvalido);
        }

        recuperacion.FechaVerificacion = DateTime.UtcNow;
        recuperacion.ExpiraEn = DateTime.UtcNow.AddMinutes(RecuperacionExpiraEnMinutos);
        await _recuperacionesPassword.GuardarCambiosAsync(ct);

        return Resultado<VerificarRecuperacionPasswordResponseDto>.Ok(new VerificarRecuperacionPasswordResponseDto
        {
            RecoveryToken = recuperacion.TokenRecuperacion
        }, "Código verificado correctamente.");
    }

    public async Task<Resultado<object>> CambiarPasswordAsync(CambiarPasswordRequestDto dto, CancellationToken ct = default)
    {
        var email = NormalizarEmail(dto.Email);
        var recuperacion = await _recuperacionesPassword.ObtenerVerificadaPorTokenAsync(dto.RecoveryToken.Trim(), ct);
        if (recuperacion is null || recuperacion.Email != email)
            return Resultado<object>.Error("La recuperación expiró. Solicita un nuevo código.");

        var usuario = await _usuarios.ObtenerPorEmailAsync(email, ct);
        if (usuario is null || usuario.IdUsuario != recuperacion.IdUsuario || !usuario.Activo)
            return Resultado<object>.Error("La recuperación expiró. Solicita un nuevo código.");

        usuario.PasswordHash = _hasher.Hash(dto.NuevaPassword);
        recuperacion.Usado = true;
        await _usuarios.GuardarCambiosAsync(ct);

        return Resultado<object>.Ok(new { }, "Contraseña actualizada correctamente.");
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
            return Resultado<RegistroResponseDto>.Error("La verificación del correo expiró. Vuelve a confirmar tu correo.");

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
        await _perfiles.AgregarAsync(new PerfilUsuario
        {
            Usuario = usuario,
            NombrePerfil = nombrePerfil,
            FechaNacimiento = dto.FechaNacimiento.Value
        }, ct);
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
        var roles = usuario.Roles
            .Select(ur => ur.Rol?.Nombre)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .DefaultIfEmpty(usuario.Rol)
            .Select(r => r.ToLowerInvariant())
            .ToList();

        var rolPrincipal = roles.Contains(usuario.Rol.ToLowerInvariant())
            ? usuario.Rol.ToLowerInvariant()
            : roles.First();

        return new AuthResponseDto
        {
            IdUsuario = usuario.IdUsuario,
            NombreUsuario = usuario.NombreUsuario,
            NombrePerfil = usuario.NombrePerfil,
            Email = usuario.Email,
            Rol = rolPrincipal,
            Roles = roles,
            EsFundador = string.Equals(usuario.Email, "craul0090@gmail.com", StringComparison.OrdinalIgnoreCase),
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
