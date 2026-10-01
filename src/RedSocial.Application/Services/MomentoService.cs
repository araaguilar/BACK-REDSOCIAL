using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Momentos;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Application.Interfaces.Services;
using RedSocial.Domain.Entities;

namespace RedSocial.Application.Services;

public class MomentoService : IMomentoService
{
    private static readonly string[] TiposPermitidos = ["foto", "video", "link"];

    private readonly IMomentoRepository _momentos;
    private readonly IUsuarioRepository _usuarios;

    public MomentoService(IMomentoRepository momentos, IUsuarioRepository usuarios)
    {
        _momentos = momentos;
        _usuarios = usuarios;
    }

    public async Task<Resultado<MomentoFeedDto>> CrearAsync(int idUsuario, CrearMomentoDto request, CancellationToken ct = default)
    {
        var texto = request.Texto.Trim();
        if (string.IsNullOrWhiteSpace(texto))
            return Resultado<MomentoFeedDto>.Error("El momento necesita un texto.");

        if (texto.Length > 180)
            return Resultado<MomentoFeedDto>.Error("El momento no puede superar los 180 caracteres.");

        if (!await _usuarios.ExistePorIdAsync(idUsuario, ct))
            return Resultado<MomentoFeedDto>.Error("No se encontro el usuario del momento.");

        var tipoAdjunto = string.IsNullOrWhiteSpace(request.TipoAdjunto) ? null : request.TipoAdjunto.Trim().ToLowerInvariant();
        if (tipoAdjunto is not null && !TiposPermitidos.Contains(tipoAdjunto))
            return Resultado<MomentoFeedDto>.Error("El tipo de adjunto no es valido.");

        if (tipoAdjunto == "link" && string.IsNullOrWhiteSpace(request.LinkUrl))
            return Resultado<MomentoFeedDto>.Error("Agrega un enlace valido para este momento.");

        var momento = new Momento
        {
            IdUsuario = idUsuario,
            Texto = texto,
            TipoAdjunto = tipoAdjunto,
            ArchivoUrl = string.IsNullOrWhiteSpace(request.ArchivoUrl) ? null : request.ArchivoUrl,
            LinkUrl = string.IsNullOrWhiteSpace(request.LinkUrl) ? null : request.LinkUrl.Trim(),
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        await _momentos.AgregarAsync(momento, ct);
        await _momentos.GuardarCambiosAsync(ct);

        var feed = await _momentos.ObtenerFeedAsync(1, ct);
        var creado = feed.FirstOrDefault(m => m.IdMomento == momento.IdMomento) ?? new MomentoFeedDto
        {
            IdMomento = momento.IdMomento,
            IdUsuario = momento.IdUsuario,
            Texto = momento.Texto,
            TipoAdjunto = momento.TipoAdjunto,
            ArchivoUrl = momento.ArchivoUrl,
            LinkUrl = momento.LinkUrl,
            FechaCreacion = momento.FechaCreacion
        };

        return Resultado<MomentoFeedDto>.Ok(creado, "Momento creado.");
    }

    public async Task<Resultado<List<MomentoFeedDto>>> ObtenerFeedAsync(CancellationToken ct = default)
    {
        var feed = await _momentos.ObtenerFeedAsync(30, ct);
        return Resultado<List<MomentoFeedDto>>.Ok(feed);
    }
}
