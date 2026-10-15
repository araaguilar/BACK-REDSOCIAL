using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedSocial.Application.DTOs.Perfil;
using RedSocial.Application.Interfaces.Services;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PerfilController : ControllerBase
{
    private static readonly string[] ExtensionesImagen = [".jpg", ".jpeg", ".png", ".webp", ".heic", ".heif"];
    private readonly IPerfilService _perfilService;
    private readonly IWebHostEnvironment _environment;
    private readonly AppDbContext _context;

    public PerfilController(IPerfilService perfilService, IWebHostEnvironment environment, AppDbContext context)
    {
        _perfilService = perfilService;
        _environment = environment;
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var resultado = await _perfilService.ObtenerMiPerfilAsync(idUsuario, ct);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("{nombreUsuario}")]
    public async Task<IActionResult> Publico(string nombreUsuario, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var resultado = await _perfilService.ObtenerPerfilPublicoAsync(nombreUsuario, idUsuario, ct);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpPost("{idUsuario:int}/seguimiento")]
    public async Task<IActionResult> AlternarSeguimiento(int idUsuario, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idSeguidor)) return Unauthorized();

        var resultado = await _perfilService.AlternarSeguimientoAsync(idSeguidor, idUsuario, ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpGet("{idUsuario:int}/seguidores")]
    public async Task<IActionResult> Seguidores(int idUsuario, [FromQuery] int pagina = 1, CancellationToken ct = default)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuarioActual)) return Unauthorized();

        const int cantidad = 10;
        pagina = Math.Max(1, pagina);

        var query =
            from relacion in _context.Seguidores.AsNoTracking()
            join usuario in _context.Usuarios.AsNoTracking() on relacion.IdSeguidor equals usuario.IdUsuario
            join perfil in _context.PerfilesUsuario.AsNoTracking() on usuario.IdUsuario equals perfil.IdUsuario into perfiles
            from perfil in perfiles.DefaultIfEmpty()
            where relacion.IdSeguido == idUsuario && usuario.Activo
            orderby relacion.FechaSeguimiento descending
            select new
            {
                idUsuario = usuario.IdUsuario,
                usuario = "@" + usuario.NombreUsuario,
                nombre = perfil != null ? perfil.NombrePerfil : usuario.NombrePerfil,
                fotoPerfilUrl = perfil != null ? perfil.FotoPerfilUrl : null,
                siguiendo = _context.Seguidores.Any(s => s.IdSeguidor == idUsuarioActual && s.IdSeguido == usuario.IdUsuario)
            };

        return Ok(new { exito = true, datos = await PaginarAsync(query, pagina, cantidad, ct) });
    }

    [HttpGet("{idUsuario:int}/seguidos")]
    public async Task<IActionResult> Seguidos(int idUsuario, [FromQuery] int pagina = 1, CancellationToken ct = default)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuarioActual)) return Unauthorized();

        const int cantidad = 10;
        pagina = Math.Max(1, pagina);

        var query =
            from relacion in _context.Seguidores.AsNoTracking()
            join usuario in _context.Usuarios.AsNoTracking() on relacion.IdSeguido equals usuario.IdUsuario
            join perfil in _context.PerfilesUsuario.AsNoTracking() on usuario.IdUsuario equals perfil.IdUsuario into perfiles
            from perfil in perfiles.DefaultIfEmpty()
            where relacion.IdSeguidor == idUsuario && usuario.Activo
            orderby relacion.FechaSeguimiento descending
            select new
            {
                idUsuario = usuario.IdUsuario,
                usuario = "@" + usuario.NombreUsuario,
                nombre = perfil != null ? perfil.NombrePerfil : usuario.NombrePerfil,
                fotoPerfilUrl = perfil != null ? perfil.FotoPerfilUrl : null,
                siguiendo = _context.Seguidores.Any(s => s.IdSeguidor == idUsuarioActual && s.IdSeguido == usuario.IdUsuario)
            };

        return Ok(new { exito = true, datos = await PaginarAsync(query, pagina, cantidad, ct) });
    }

    [HttpPut("sobre-mi")]
    public async Task<IActionResult> ActualizarSobreMi([FromBody] ActualizarSobreMiRequestDto request, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var resultado = await _perfilService.ActualizarSobreMiAsync(idUsuario, request.SobreMi, ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPut("nombre-perfil")]
    public async Task<IActionResult> ActualizarNombrePerfil([FromBody] ActualizarNombrePerfilRequestDto request, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var resultado = await _perfilService.ActualizarNombrePerfilAsync(idUsuario, request.NombrePerfil, ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPut("nombre-usuario")]
    public async Task<IActionResult> ActualizarNombreUsuario([FromBody] ActualizarNombreUsuarioRequestDto request, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var resultado = await _perfilService.ActualizarNombreUsuarioAsync(idUsuario, request.NombreUsuario, ct);
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

    [HttpDelete("foto")]
    public async Task<IActionResult> EliminarFoto(CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var perfil = await _context.PerfilesUsuario.FirstOrDefaultAsync(p => p.IdUsuario == idUsuario, ct);
        if (perfil is null) return NotFound(new { exito = false, mensaje = "No se encontró el perfil." });

        perfil.FotoPerfilUrl = null;
        perfil.FechaActualizacion = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        var resultado = await _perfilService.ObtenerMiPerfilAsync(idUsuario, ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    private static async Task<object> PaginarAsync<T>(IQueryable<T> query, int pagina, int cantidad, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pagina - 1) * cantidad)
            .Take(cantidad)
            .ToListAsync(ct);

        return new
        {
            items,
            pagina,
            total,
            totalPaginas = (int)Math.Ceiling(total / (double)cantidad),
            cantidad
        };
    }

    private async Task<(string? Url, string? Error)> GuardarFotoPerfilAsync(IFormFile? foto, CancellationToken ct)
    {
        if (foto is null || foto.Length == 0)
            return (null, "Selecciona una foto para tu perfil.");

        var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();
        if (!ExtensionesImagen.Contains(extension))
            return (null, "La foto debe ser JPG, PNG, WEBP o HEIC.");

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
