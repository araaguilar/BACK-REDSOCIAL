using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedSocial.Application.DTOs.Momentos;
using RedSocial.Application.Interfaces.Services;

namespace RedSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MomentosController : ControllerBase
{
    private static readonly string[] ExtensionesImagen = [".jpg", ".jpeg", ".png", ".webp"];
    private static readonly string[] ExtensionesVideo = [".mp4", ".webm", ".mov"];
    private readonly IMomentoService _momentoService;
    private readonly IWebHostEnvironment _environment;

    public MomentosController(IMomentoService momentoService, IWebHostEnvironment environment)
    {
        _momentoService = momentoService;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> Feed(CancellationToken ct)
    {
        var resultado = await _momentoService.ObtenerFeedAsync(ct);
        return Ok(resultado);
    }

    [HttpGet("me")]
    public async Task<IActionResult> MisMomentos([FromQuery] int? cursor, [FromQuery] int cantidad = 30, CancellationToken ct = default)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var resultado = await _momentoService.ObtenerMisMomentosAsync(idUsuario, cursor, cantidad, ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost]
    [RequestSizeLimit(15 * 1024 * 1024)]
    public async Task<IActionResult> Crear([FromForm] CrearMomentoForm request, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var tipoAdjunto = string.IsNullOrWhiteSpace(request.TipoAdjunto)
            ? null
            : request.TipoAdjunto.Trim().ToLowerInvariant();

        var archivoUrl = await GuardarArchivoAsync(request.Archivo, tipoAdjunto, ct);
        if (archivoUrl.Error is not null)
            return BadRequest(new { exito = false, mensaje = archivoUrl.Error });

        var dto = new CrearMomentoDto
        {
            Texto = request.Texto,
            TipoAdjunto = tipoAdjunto,
            ArchivoUrl = archivoUrl.Url,
            LinkUrl = request.LinkUrl
        };

        var resultado = await _momentoService.CrearAsync(idUsuario, dto, ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    private async Task<(string? Url, string? Error)> GuardarArchivoAsync(IFormFile? archivo, string? tipoAdjunto, CancellationToken ct)
    {
        if (archivo is null || archivo.Length == 0)
        {
            if (tipoAdjunto is "foto" or "video")
                return (null, "Selecciona el archivo que quieres compartir.");

            return (null, null);
        }

        if (tipoAdjunto is not ("foto" or "video"))
            return (null, "Selecciona si el archivo es foto o video.");

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var extensionesPermitidas = tipoAdjunto == "foto" ? ExtensionesImagen : ExtensionesVideo;
        if (!extensionesPermitidas.Contains(extension))
            return (null, "El archivo no tiene un formato permitido.");

        var contentTypeValido = tipoAdjunto == "foto"
            ? archivo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
            : archivo.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);

        if (!contentTypeValido)
            return (null, "El tipo de archivo no coincide con el adjunto seleccionado.");

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var carpeta = Path.Combine(webRoot, "uploads", "momentos");
        Directory.CreateDirectory(carpeta);

        var nombreSeguro = $"{Guid.NewGuid():N}{extension}";
        var ruta = Path.Combine(carpeta, nombreSeguro);

        await using var stream = System.IO.File.Create(ruta);
        await archivo.CopyToAsync(stream, ct);

        return ($"/uploads/momentos/{nombreSeguro}", null);
    }
}

public class CrearMomentoForm
{
    public string Texto { get; set; } = string.Empty;
    public string? TipoAdjunto { get; set; }
    public string? LinkUrl { get; set; }
    public IFormFile? Archivo { get; set; }
}
