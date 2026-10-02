using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdvertenciasController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdvertenciasController(AppDbContext context) => _context = context;

    [HttpGet("pendientes")]
    public async Task<IActionResult> Pendientes(CancellationToken ct)
    {
        var idUsuario = ObtenerIdUsuario();
        if (idUsuario is null) return Unauthorized();

        var advertencias = await (
            from advertencia in _context.AdvertenciasUsuario.AsNoTracking()
            join moderador in _context.Usuarios.AsNoTracking() on advertencia.IdModerador equals moderador.IdUsuario
            join perfilModerador in _context.PerfilesUsuario.AsNoTracking() on moderador.IdUsuario equals perfilModerador.IdUsuario into perfilesModerador
            from perfilModerador in perfilesModerador.DefaultIfEmpty()
            where advertencia.IdUsuario == idUsuario && !advertencia.Leida
            orderby advertencia.FechaCreacion descending
            select new
            {
                idAdvertencia = advertencia.IdAdvertencia,
                plantilla = advertencia.Plantilla,
                mensaje = advertencia.Mensaje,
                fechaCreacion = advertencia.FechaCreacion,
                moderador = perfilModerador != null ? perfilModerador.NombrePerfil : moderador.NombrePerfil
            }).Take(3).ToListAsync(ct);

        return Ok(new { exito = true, datos = advertencias });
    }

    [HttpPost("{idAdvertencia:int}/leer")]
    public async Task<IActionResult> MarcarLeida(int idAdvertencia, CancellationToken ct)
    {
        var idUsuario = ObtenerIdUsuario();
        if (idUsuario is null) return Unauthorized();

        var advertencia = await _context.AdvertenciasUsuario
            .FirstOrDefaultAsync(a => a.IdAdvertencia == idAdvertencia && a.IdUsuario == idUsuario, ct);

        if (advertencia is null) return NotFound(new { exito = false, mensaje = "Advertencia no encontrada." });

        advertencia.Leida = true;
        await _context.SaveChangesAsync(ct);

        return Ok(new { exito = true, mensaje = "Advertencia leída." });
    }

    private int? ObtenerIdUsuario()
    {
        var idClaim = User.FindFirst("sub")?.Value;
        return int.TryParse(idClaim, out var idUsuario) ? idUsuario : null;
    }
}
