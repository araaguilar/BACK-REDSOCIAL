using RedSocial.Application.DTOs.Momentos;
using RedSocial.Domain.Entities;

namespace RedSocial.Application.Interfaces.Persistence;

public interface IMomentoRepository
{
    Task AgregarAsync(Momento momento, CancellationToken ct = default);
    Task<List<MomentoFeedDto>> ObtenerFeedAsync(int idUsuarioActual, int cantidad = 30, CancellationToken ct = default);
    Task<MomentosPaginadosDto> ObtenerPorUsuarioAsync(int idUsuario, int idUsuarioActual, int? cursor, int cantidad = 30, CancellationToken ct = default);
    Task<MomentoFeedDto?> ObtenerPorIdAsync(int idMomento, int idUsuarioActual, CancellationToken ct = default);
    Task<MeGustaMomentoDto?> AlternarMeGustaAsync(int idMomento, int idUsuario, CancellationToken ct = default);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
