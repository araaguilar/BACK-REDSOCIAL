using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Busqueda;

namespace RedSocial.Application.Interfaces.Services;

public interface IBusquedaService
{
    Task<Resultado<ResultadoBusquedaDto>> BuscarAsync(string? termino, int idUsuarioActual, CancellationToken ct = default);
}
