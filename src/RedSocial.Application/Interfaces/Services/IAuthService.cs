using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Auth;

namespace RedSocial.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Resultado<AuthResponseDto>> LoginAsync(LoginRequestDto dto, CancellationToken ct = default);
    Task<Resultado<RegistroResponseDto>> RegistrarAsync(RegistroRequestDto dto, CancellationToken ct = default);
}
