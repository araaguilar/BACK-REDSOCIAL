using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using RedSocial.Application.DTOs.Momentos;
using RedSocial.Application.Interfaces.Services;
using RedSocial.Domain.Entities;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MomentosController : ControllerBase
{
    private static readonly string[] ExtensionesImagen = [".jpg", ".jpeg", ".png", ".webp", ".heic", ".heif"];
    private readonly IMomentoService _momentoService;
    private readonly IWebHostEnvironment _environment;
    private readonly AppDbContext _context;

    public MomentosController(IMomentoService momentoService, IWebHostEnvironment environment, AppDbContext context)
    {
        _momentoService = momentoService;
        _environment = environment;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Feed(CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var resultado = await _momentoService.ObtenerFeedAsync(idUsuario, ct);
        return Ok(resultado);
    }

    [HttpGet("publico")]
    [AllowAnonymous]
    public async Task<IActionResult> FeedPublico(CancellationToken ct)
    {
        var resultado = await _momentoService.ObtenerFeedPublicoAsync(ct);
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

    [HttpGet("usuario/{idUsuario:int}")]
    public async Task<IActionResult> MomentosDeUsuario(int idUsuario, [FromQuery] int? cursor, [FromQuery] int cantidad = 30, CancellationToken ct = default)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuarioActual)) return Unauthorized();

        var resultado = await _momentoService.ObtenerMomentosDeUsuarioAsync(idUsuario, idUsuarioActual, cursor, cantidad, ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("{idMomento:int}/me-encanta")]
    public async Task<IActionResult> AlternarMeGusta(int idMomento, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var resultado = await _momentoService.AlternarMeGustaAsync(idUsuario, idMomento, ct);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpDelete("{idMomento:int}")]
    public async Task<IActionResult> Eliminar(int idMomento, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var resultado = await _momentoService.EliminarAsync(idUsuario, idMomento, "Eliminado por el usuario", ct);
        return resultado.Exito ? Ok(resultado) : NotFound(resultado);
    }

    [HttpGet("{idMomento:int}/comentarios")]
    public async Task<IActionResult> Comentarios(int idMomento, [FromQuery] int? cursor, [FromQuery] int cantidad = 20, [FromQuery] int? padre = null, CancellationToken ct = default)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuarioActual)) return Unauthorized();

        var existeMomento = await _context.Momentos.AsNoTracking()
            .AnyAsync(m => m.IdMomento == idMomento && m.Activo, ct);
        if (!existeMomento) return NotFound(new { exito = false, mensaje = "Momento no encontrado." });

        var take = Math.Clamp(cantidad, 1, 30);
        var comentarios = await (
            from comentario in _context.ComentariosMomentos.AsNoTracking()
            join usuario in _context.Usuarios.AsNoTracking() on comentario.IdUsuario equals usuario.IdUsuario
            join perfil in _context.PerfilesUsuario.AsNoTracking() on usuario.IdUsuario equals perfil.IdUsuario into perfiles
            from perfil in perfiles.DefaultIfEmpty()
            where comentario.IdMomento == idMomento
                && comentario.IdComentarioPadre == padre
                && comentario.Activo
                && (!cursor.HasValue || comentario.IdComentario < cursor.Value)
            orderby comentario.IdComentario descending
            select new
            {
                idComentario = comentario.IdComentario,
                idMomento = comentario.IdMomento,
                idUsuario = comentario.IdUsuario,
                autor = perfil != null ? perfil.NombrePerfil : usuario.NombrePerfil,
                usuario = "@" + usuario.NombreUsuario,
                avatar = perfil != null && perfil.FotoPerfilUrl != null
                    ? perfil.FotoPerfilUrl
                    : (perfil != null ? perfil.NombrePerfil : usuario.NombreUsuario).Substring(0, Math.Min((perfil != null ? perfil.NombrePerfil : usuario.NombreUsuario).Length, 2)).ToUpper(),
                texto = comentario.Texto,
                totalMeGusta = comentario.TotalMeGusta,
                totalRespuestas = comentario.TotalRespuestas,
                leGusta = _context.MeGustaComentariosMomentos.Any(mg => mg.IdComentario == comentario.IdComentario && mg.IdUsuario == idUsuarioActual),
                fechaCreacion = comentario.FechaCreacion
            }).Take(take + 1).ToListAsync(ct);

        var tieneMas = comentarios.Count > take;
        if (tieneMas) comentarios.RemoveAt(comentarios.Count - 1);

        return Ok(new
        {
            exito = true,
            datos = new
            {
                items = comentarios,
                tieneMas,
                siguienteCursor = tieneMas ? comentarios.LastOrDefault()?.idComentario : null
            }
        });
    }

    [HttpPost("{idMomento:int}/comentarios")]
    public async Task<IActionResult> CrearComentario(int idMomento, [FromBody] CrearComentarioMomentoRequest request, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var texto = (request.Texto ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(texto))
            return BadRequest(new { exito = false, mensaje = "Escribe un comentario." });
        if (texto.Length > 300)
            return BadRequest(new { exito = false, mensaje = "El comentario no puede superar los 300 caracteres." });

        var momento = await _context.Momentos.FirstOrDefaultAsync(m => m.IdMomento == idMomento && m.Activo, ct);
        if (momento is null) return NotFound(new { exito = false, mensaje = "Momento no encontrado." });

        ComentarioMomento? comentarioPadre = null;
        if (request.IdComentarioPadre is not null)
        {
            comentarioPadre = await _context.ComentariosMomentos
                .FirstOrDefaultAsync(c => c.IdComentario == request.IdComentarioPadre && c.IdMomento == idMomento && c.Activo, ct);
            if (comentarioPadre is null)
                return NotFound(new { exito = false, mensaje = "Comentario no encontrado." });
        }

        var comentario = new ComentarioMomento
        {
            IdMomento = idMomento,
            IdUsuario = idUsuario,
            IdComentarioPadre = request.IdComentarioPadre,
            Texto = texto,
            FechaCreacion = DateTime.UtcNow
        };

        momento.TotalComentarios += 1;
        if (comentarioPadre is not null)
            comentarioPadre.TotalRespuestas += 1;

        await _context.ComentariosMomentos.AddAsync(comentario, ct);
        await _context.SaveChangesAsync(ct);

        var usuario = await _context.Usuarios.AsNoTracking()
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => new
            {
                u.IdUsuario,
                u.NombreUsuario,
                u.NombrePerfil,
                NombrePerfilReal = u.Perfil != null ? u.Perfil.NombrePerfil : null,
                FotoPerfilUrl = u.Perfil != null ? u.Perfil.FotoPerfilUrl : null
            })
            .FirstAsync(ct);

        var autor = usuario.NombrePerfilReal ?? usuario.NombrePerfil;
        return Ok(new
        {
            exito = true,
            mensaje = "Comentario publicado.",
            datos = new
            {
                idComentario = comentario.IdComentario,
                idMomento,
                idUsuario,
                idComentarioPadre = comentario.IdComentarioPadre,
                autor,
                usuario = "@" + usuario.NombreUsuario,
                avatar = usuario.FotoPerfilUrl ?? autor.Substring(0, Math.Min(autor.Length, 2)).ToUpper(),
                texto = comentario.Texto,
                totalMeGusta = comentario.TotalMeGusta,
                totalRespuestas = comentario.TotalRespuestas,
                leGusta = false,
                fechaCreacion = comentario.FechaCreacion,
                totalComentarios = momento.TotalComentarios
            }
        });
    }

    [HttpPost("comentarios/{idComentario:int}/me-encanta")]
    public async Task<IActionResult> AlternarMeGustaComentario(int idComentario, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        var comentario = await _context.ComentariosMomentos
            .FirstOrDefaultAsync(c => c.IdComentario == idComentario && c.Activo, ct);
        if (comentario is null) return NotFound(new { exito = false, mensaje = "Comentario no encontrado." });

        var existente = await _context.MeGustaComentariosMomentos
            .FirstOrDefaultAsync(mg => mg.IdComentario == idComentario && mg.IdUsuario == idUsuario, ct);

        var leGusta = existente is null;
        if (leGusta)
        {
            await _context.MeGustaComentariosMomentos.AddAsync(new ComentarioMomentoMeGusta
            {
                IdComentario = idComentario,
                IdUsuario = idUsuario,
                FechaCreacion = DateTime.UtcNow
            }, ct);
            comentario.TotalMeGusta += 1;
        }
        else
        {
            _context.MeGustaComentariosMomentos.Remove(existente!);
            comentario.TotalMeGusta = Math.Max(0, comentario.TotalMeGusta - 1);
        }

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return Ok(new
        {
            exito = true,
            datos = new
            {
                idComentario,
                leGusta,
                totalMeGusta = comentario.TotalMeGusta
            }
        });
    }

    [HttpPost]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Crear([FromForm] CrearMomentoForm request, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var tipoAdjunto = string.IsNullOrWhiteSpace(request.TipoAdjunto)
            ? null
            : request.TipoAdjunto.Trim().ToLowerInvariant();

        var archivos = await GuardarArchivosAsync(request.Archivos, tipoAdjunto, ct);
        if (archivos.Error is not null)
            return BadRequest(new { exito = false, mensaje = archivos.Error });

        var dto = new CrearMomentoDto
        {
            Texto = request.Texto,
            TipoAdjunto = tipoAdjunto,
            ArchivoUrl = archivos.Urls.Count switch
            {
                0 => null,
                1 => archivos.Urls[0],
                _ => JsonSerializer.Serialize(archivos.Urls)
            },
            LinkUrl = request.LinkUrl
        };

        var resultado = await _momentoService.CrearAsync(idUsuario, dto, ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    private async Task<(List<string> Urls, string? Error)> GuardarArchivosAsync(IFormFile[]? archivos, string? tipoAdjunto, CancellationToken ct)
    {
        var lista = (archivos ?? []).Where(a => a is not null && a.Length > 0).ToArray();
        if (lista.Length == 0)
        {
            if (tipoAdjunto == "foto")
                return ([], "Selecciona al menos una imagen para compartir.");

            return ([], null);
        }

        if (tipoAdjunto != "foto")
            return ([], "Solo se permiten imágenes en los momentos.");
        if (lista.Length > 10)
            return ([], "Puedes subir hasta 10 imágenes por publicación.");

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var carpeta = Path.Combine(webRoot, "uploads", "momentos");
        Directory.CreateDirectory(carpeta);
        var urls = new List<string>(lista.Length);

        foreach (var archivo in lista)
        {
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesImagen.Contains(extension) || !archivo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return ([], "Todas las imágenes deben ser JPG, PNG, WEBP o HEIC.");

            var nombreSeguro = $"{Guid.NewGuid():N}{extension}";
            var ruta = Path.Combine(carpeta, nombreSeguro);
            await using var stream = System.IO.File.Create(ruta);
            await archivo.CopyToAsync(stream, ct);
            urls.Add($"/uploads/momentos/{nombreSeguro}");
        }

        return (urls, null);
    }
}

public class CrearMomentoForm
{
    public string Texto { get; set; } = string.Empty;
    public string? TipoAdjunto { get; set; }
    public string? LinkUrl { get; set; }
    public IFormFile[] Archivos { get; set; } = [];
}

public class CrearComentarioMomentoRequest
{
    public string Texto { get; set; } = string.Empty;
    public int? IdComentarioPadre { get; set; }
}
