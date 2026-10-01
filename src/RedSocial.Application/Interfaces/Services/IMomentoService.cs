using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Momentos;

namespace RedSocial.Application.Interfaces.Services;

public interface IMomentoService
{
    Task<Resultado<MomentoFeedDto>> CrearAsync(int idUsuario, CrearMomentoDto request, CancellationToken ct = default);
    Task<Resultado<List<MomentoFeedDto>>> ObtenerFeedAsync(CancellationToken ct = default);
}
