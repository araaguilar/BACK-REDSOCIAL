using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedSocial.Application.DTOs.Perfil;
using RedSocial.Application.Interfaces.Services;

namespace RedSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PerfilController : ControllerBase
{
    private static readonly string[] ExtensionesImagen = [".jpg", ".jpeg", ".png", ".webp"];
    private readonly IPerfilService _perfilService;
    private readonly IWebHostEnvironment _environment;

    public PerfilController(IPerfilService perfilService, IWebHostEnvironment environment)
    {
        _perfilService = perfilService;
        _environment = environment;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var resultado = await _perfilService.ObtenerMiPerfilAsync(idUsuario, ct);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPut("sobre-mi")]
    public async Task<IActionResult> ActualizarSobreMi([FromBody] ActualizarSobreMiRequestDto request, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var resultado = await _perfilService.ActualizarSobreMiAsync(idUsuario, request.SobreMi, ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("foto")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> ActualizarFoto([FromForm] FotoPerfilForm request, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var archivoUrl = await GuardarFotoPerfilAsync(request.Foto, ct);
        if (archivoUrl.Error is not null)
            return BadRequest(new { exito = false, mensaje = archivoUrl.Error });

        var resultado = await _perfilService.ActualizarFotoPerfilAsync(idUsuario, archivoUrl.Url!, ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    private async Task<(string? Url, string? Error)> GuardarFotoPerfilAsync(IFormFile? foto, CancellationToken ct)
    {
        if (foto is null || foto.Length == 0)
            return (null, "Selecciona una foto para tu perfil.");

        var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();
        if (!ExtensionesImagen.Contains(extension))
            return (null, "La foto debe ser JPG, PNG o WEBP.");

        if (!foto.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return (null, "El archivo seleccionado no parece ser una imagen.");

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var carpeta = Path.Combine(webRoot, "uploads", "perfiles");
        Directory.CreateDirectory(carpeta);

        var nombreSeguro = $"{Guid.NewGuid():N}{extension}";
        var ruta = Path.Combine(carpeta, nombreSeguro);

        await using var stream = System.IO.File.Create(ruta);
        await foto.CopyToAsync(stream, ct);

        return ($"/uploads/perfiles/{nombreSeguro}", null);
    }
}

public class FotoPerfilForm
{
    public IFormFile? Foto { get; set; }
}
