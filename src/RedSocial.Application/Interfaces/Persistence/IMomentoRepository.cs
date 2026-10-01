using RedSocial.Application.DTOs.Momentos;
using RedSocial.Domain.Entities;

namespace RedSocial.Application.Interfaces.Persistence;

public interface IMomentoRepository
{
    Task AgregarAsync(Momento momento, CancellationToken ct = default);
    Task<List<MomentoFeedDto>> ObtenerFeedAsync(int cantidad = 30, CancellationToken ct = default);
    Task GuardarCambiosAsync(CancellationToken ct = default);
}
