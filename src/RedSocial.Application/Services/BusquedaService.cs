using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Busqueda;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Application.Interfaces.Services;

namespace RedSocial.Application.Services;

public class BusquedaService : IBusquedaService
{
    private readonly IBusquedaRepository _busqueda;

    public BusquedaService(IBusquedaRepository busqueda) => _busqueda = busqueda;

    public async Task<Resultado<ResultadoBusquedaDto>> BuscarAsync(string? termino, int idUsuarioActual, CancellationToken ct = default)
    {
        var q = termino?.Trim().TrimStart('@') ?? string.Empty;

        var perfiles = await _busqueda.BuscarPerfilesAsync(q, 12, ct);
        var momentos = await _busqueda.BuscarMomentosAsync(q, idUsuarioActual, 18, ct);

        return Resultado<ResultadoBusquedaDto>.Ok(new ResultadoBusquedaDto
        {
            Perfiles = perfiles,
            Momentos = momentos
        });
    }
}
