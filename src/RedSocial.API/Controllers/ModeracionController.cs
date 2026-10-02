using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedSocial.Domain.Entities;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "moderador,admin")]
public class ModeracionController : ControllerBase
{
    private readonly AppDbContext _context;

    public ModeracionController(AppDbContext context) => _context = context;

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var reportesAbiertos = await _context.Reportes.CountAsync(r => r.Estado == "abierto", ct);
        var alertasSpam = await _context.Reportes.CountAsync(r => r.Estado == "abierto" && r.Motivo == "spam", ct);
        var cuentasRestringidas = await _context.ModeracionAcciones.CountAsync(a => a.Tipo == "restringir", ct);
        var casosUrgentes = await _context.Reportes.CountAsync(r => r.Estado == "abierto" && (r.Motivo == "violencia" || r.Motivo == "odio"), ct);

        var cuentas = await (
            from reporte in _context.Reportes.AsNoTracking()
            join usuario in _context.Usuarios.AsNoTracking() on reporte.IdUsuarioReportado equals usuario.IdUsuario
            join perfil in _context.PerfilesUsuario.AsNoTracking() on usuario.IdUsuario equals perfil.IdUsuario into perfiles
            from perfil in perfiles.DefaultIfEmpty()
            where reporte.Estado == "abierto" && reporte.IdUsuarioReportado != null
            group reporte by new
            {
                usuario.IdUsuario,
                usuario.NombreUsuario,
                NombrePerfil = perfil != null ? perfil.NombrePerfil : usuario.NombrePerfil
            } into g
            orderby g.Count() descending
            select new
            {
                idUsuario = g.Key.IdUsuario,
                usuario = "@" + g.Key.NombreUsuario,
                nombre = g.Key.NombrePerfil,
                reportes = g.Count(),
                motivo = g.GroupBy(r => r.Motivo).OrderByDescending(m => m.Count()).Select(m => m.Key).First(),
                riesgo = g.Count() >= 5 ? "Alto" : "Medio"
            }).Take(8).ToListAsync(ct);

        var spam = await _context.Reportes
            .AsNoTracking()
            .Where(r => r.Estado == "abierto" && r.Motivo == "spam")
            .OrderByDescending(r => r.FechaCreacion)
            .Take(6)
            .Select(r => new { id = r.IdReporte, titulo = "Reporte por spam", detalle = r.Detalle ?? "Contenido marcado como spam.", nivel = "Medio" })
            .ToListAsync(ct);

        return Ok(new
        {
            exito = true,
            datos = new
            {
                estadisticas = new { reportesAbiertos, alertasSpam, cuentasRestringidas, casosUrgentes },
                cuentasReportadas = cuentas,
                alertasSpam = spam
            }
        });
    }

    [HttpPost("usuarios/{idUsuario:int}/accion")]
    public async Task<IActionResult> AccionUsuario(int idUsuario, [FromBody] ModeracionAccionRequest request, CancellationToken ct)
    {
        var idModerador = ObtenerIdUsuario();
        if (idModerador is null) return Unauthorized();

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);
        if (usuario is null) return NotFound(new { exito = false, mensaje = "Usuario no encontrado." });

        var tipo = request.Tipo.Trim().ToLowerInvariant();
        if (tipo is not ("advertencia" or "restringir" or "desactivar" or "eliminar"))
            return BadRequest(new { exito = false, mensaje = "Acción no válida." });

        if (tipo is "desactivar" or "eliminar")
        {
            usuario.Activo = false;
            usuario.MotivoEstadoCuenta = request.Motivo;
            if (tipo == "desactivar") usuario.FechaDesactivacion = DateTime.UtcNow;
            if (tipo == "eliminar") usuario.FechaEliminacion = DateTime.UtcNow;
        }

        await _context.ModeracionAcciones.AddAsync(new ModeracionAccion
        {
            IdModerador = idModerador.Value,
            IdUsuarioObjetivo = idUsuario,
            Tipo = tipo,
            Motivo = request.Motivo,
            Mensaje = request.Mensaje,
            FechaCreacion = DateTime.UtcNow
        }, ct);

        if (tipo == "advertencia")
        {
            await _context.AdvertenciasUsuario.AddAsync(new AdvertenciaUsuario
            {
                IdUsuario = idUsuario,
                IdModerador = idModerador.Value,
                Plantilla = request.Plantilla ?? "general",
                Mensaje = request.Mensaje ?? request.Motivo,
                FechaCreacion = DateTime.UtcNow
            }, ct);
        }

        await _context.SaveChangesAsync(ct);
        return Ok(new { exito = true, mensaje = "Acción registrada." });
    }

    private int? ObtenerIdUsuario()
    {
        var idClaim = User.FindFirst("sub")?.Value;
        return int.TryParse(idClaim, out var idUsuario) ? idUsuario : null;
    }
}

public class ModeracionAccionRequest
{
    public string Tipo { get; set; } = string.Empty;
    public string Motivo { get; set; } = "Acción de moderación";
    public string? Plantilla { get; set; }
    public string? Mensaje { get; set; }
}
