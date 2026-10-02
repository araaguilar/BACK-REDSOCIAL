using RedSocial.Application.DTOs.Busqueda;
using RedSocial.Application.DTOs.Momentos;

namespace RedSocial.Application.Interfaces.Persistence;

public interface IBusquedaRepository
{
    Task<List<PerfilBusquedaDto>> BuscarPerfilesAsync(string termino, int cantidad = 12, CancellationToken ct = default);
    Task<List<MomentoFeedDto>> BuscarMomentosAsync(string termino, int idUsuarioActual, int cantidad = 18, CancellationToken ct = default);
}
