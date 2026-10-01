using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Auth;

namespace RedSocial.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Resultado<SolicitarCodigoEmailResponseDto>> SolicitarCodigoEmailAsync(SolicitarCodigoEmailRequestDto dto, CancellationToken ct = default);
    Task<Resultado<VerificarCodigoEmailResponseDto>> VerificarCodigoEmailAsync(VerificarCodigoEmailRequestDto dto, CancellationToken ct = default);
    Task<Resultado<object>> CancelarCodigoEmailAsync(SolicitarCodigoEmailRequestDto dto, CancellationToken ct = default);
    Task<Resultado<DisponibilidadEmailResponseDto>> VerificarDisponibilidadEmailAsync(string email, CancellationToken ct = default);
    Task<Resultado<DisponibilidadUsuarioResponseDto>> VerificarDisponibilidadUsuarioAsync(string nombreUsuario, CancellationToken ct = default);
    Task<Resultado<AuthResponseDto>> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<Resultado<RegistroResponseDto>> RegistrarAsync(RegistroRequestDto dto, CancellationToken ct = default);
}
