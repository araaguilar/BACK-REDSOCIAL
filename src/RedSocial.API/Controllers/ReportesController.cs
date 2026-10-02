using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedSocial.Domain.Entities;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportesController : ControllerBase
{
    private static readonly string[] MotivosPermitidos = ["spam", "acoso", "odio", "suplantacion", "contenido_inapropiado", "violencia", "otro"];
    private readonly AppDbContext _context;

    public ReportesController(AppDbContext context) => _context = context;

    [HttpPost("perfil")]
    public async Task<IActionResult> ReportarPerfil([FromBody] ReporteRequest request, CancellationToken ct)
    {
        var idReportante = ObtenerIdUsuario();
        if (idReportante is null) return Unauthorized();
        if (request.IdUsuario is null || request.IdUsuario == idReportante) return BadRequest(new { exito = false, mensaje = "No puedes reportar este perfil." });
        var existeUsuario = await _context.Usuarios.AnyAsync(u => u.IdUsuario == request.IdUsuario && u.Activo, ct);
        if (!existeUsuario) return NotFound(new { exito = false, mensaje = "Perfil no encontrado." });

        var reporte = CrearReporte(idReportante.Value, "perfil", request.Motivo, request.Detalle);
        reporte.IdUsuarioReportado = request.IdUsuario;
        await _context.Reportes.AddAsync(reporte, ct);
        await _context.SaveChangesAsync(ct);
        return Ok(new { exito = true, mensaje = "Reporte enviado." });
    }

    [HttpPost("momento")]
    public async Task<IActionResult> ReportarMomento([FromBody] ReporteRequest request, CancellationToken ct)
    {
        var idReportante = ObtenerIdUsuario();
        if (idReportante is null) return Unauthorized();
        if (request.IdMomento is null) return BadRequest(new { exito = false, mensaje = "Selecciona el momento a reportar." });

        var momento = await _context.Momentos
            .AsNoTracking()
            .Where(m => m.IdMomento == request.IdMomento && m.Activo)
            .Select(m => new { m.IdMomento, m.IdUsuario })
            .FirstOrDefaultAsync(ct);
        if (momento is null) return NotFound(new { exito = false, mensaje = "Momento no encontrado." });
        if (momento.IdUsuario == idReportante) return BadRequest(new { exito = false, mensaje = "No puedes reportar tu propio momento." });

        var reporte = CrearReporte(idReportante.Value, "momento", request.Motivo, request.Detalle);
        reporte.IdMomentoReportado = momento.IdMomento;
        reporte.IdUsuarioReportado = momento.IdUsuario;
        await _context.Reportes.AddAsync(reporte, ct);
        await _context.SaveChangesAsync(ct);
        return Ok(new { exito = true, mensaje = "Reporte enviado." });
    }

    private Reporte CrearReporte(int idReportante, string tipo, string motivo, string? detalle)
    {
        var motivoNormalizado = string.IsNullOrWhiteSpace(motivo) ? "otro" : motivo.Trim().ToLowerInvariant();
        if (!MotivosPermitidos.Contains(motivoNormalizado)) motivoNormalizado = "otro";

        return new Reporte
        {
            IdReportante = idReportante,
            Tipo = tipo,
            Motivo = motivoNormalizado,
            Detalle = string.IsNullOrWhiteSpace(detalle) ? null : detalle.Trim(),
            Estado = "abierto",
            FechaCreacion = DateTime.UtcNow
        };
    }

    private int? ObtenerIdUsuario()
    {
        var idClaim = User.FindFirst("sub")?.Value;
        return int.TryParse(idClaim, out var idUsuario) ? idUsuario : null;
    }
}

public class ReporteRequest
{
    public int? IdUsuario { get; set; }
    public int? IdMomento { get; set; }
    public string Motivo { get; set; } = "otro";
    public string? Detalle { get; set; }
}
