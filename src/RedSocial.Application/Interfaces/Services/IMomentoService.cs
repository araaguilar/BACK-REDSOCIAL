using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Momentos;

namespace RedSocial.Application.Interfaces.Services;

public interface IMomentoService
{
    Task<Resultado<MomentoFeedDto>> CrearAsync(int idUsuario, CrearMomentoDto request, CancellationToken ct = default);
    Task<Resultado<List<MomentoFeedDto>>> ObtenerFeedAsync(int idUsuarioActual, CancellationToken ct = default);
    Task<Resultado<MomentosPaginadosDto>> ObtenerMisMomentosAsync(int idUsuario, int? cursor, int cantidad = 30, CancellationToken ct = default);
    Task<Resultado<MeGustaMomentoDto>> AlternarMeGustaAsync(int idUsuario, int idMomento, CancellationToken ct = default);
    Task<Resultado<object>> EliminarAsync(int idUsuario, int idMomento, string? motivo = null, CancellationToken ct = default);
}
