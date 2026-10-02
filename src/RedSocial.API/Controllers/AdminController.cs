using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedSocial.Domain.Entities;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context) => _context = context;

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var ahora = DateTime.UtcNow;
        var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);
        var inicioSemana = ahora.AddDays(-7);

        var totalUsuarios = await _context.Usuarios.CountAsync(ct);
        var usuariosActivos = await _context.Usuarios.CountAsync(u => u.Activo, ct);
        var nuevosMes = await _context.Usuarios.CountAsync(u => u.FechaRegistro >= inicioMes, ct);
        var loginsSemana = await _context.Usuarios.CountAsync(u => u.UltimoLogin != null && u.UltimoLogin >= inicioSemana, ct);
        var publicaciones = await _context.Momentos.CountAsync(m => m.Activo, ct);
        var reportesAbiertos = await _context.Reportes.CountAsync(r => r.Estado == "abierto", ct);

        var crecimiento = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.FechaRegistro >= ahora.AddDays(-30))
            .GroupBy(u => u.FechaRegistro.Date)
            .Select(g => new { fecha = g.Key, total = g.Count() })
            .OrderBy(g => g.fecha)
            .ToListAsync(ct);

        var actividad = await _context.Momentos
            .AsNoTracking()
            .Where(m => m.FechaCreacion >= ahora.AddDays(-7))
            .GroupBy(m => m.FechaCreacion.Date)
            .Select(g => new { fecha = g.Key, total = g.Count() })
            .OrderBy(g => g.fecha)
            .ToListAsync(ct);

        return Ok(new
        {
            exito = true,
            datos = new
            {
                estadisticas = new { totalUsuarios, usuariosActivos, nuevosMes, loginsSemana, publicaciones, reportesAbiertos },
                crecimiento,
                actividad
            }
        });
    }

    [HttpGet("usuarios")]
    public async Task<IActionResult> Usuarios([FromQuery] int pagina = 1, [FromQuery] string? q = null, CancellationToken ct = default)
    {
        const int cantidad = 10;
        pagina = Math.Max(1, pagina);
        q = q?.Trim();

        var query =
            from usuario in _context.Usuarios.AsNoTracking()
            join perfil in _context.PerfilesUsuario.AsNoTracking() on usuario.IdUsuario equals perfil.IdUsuario into perfiles
            from perfil in perfiles.DefaultIfEmpty()
            where string.IsNullOrWhiteSpace(q)
                || usuario.NombreUsuario.Contains(q)
                || usuario.Email.Contains(q)
                || usuario.NombrePerfil.Contains(q)
                || (perfil != null && perfil.NombrePerfil.Contains(q))
            orderby usuario.FechaRegistro descending
            select new
            {
                idUsuario = usuario.IdUsuario,
                usuario = "@" + usuario.NombreUsuario,
                nombre = perfil != null ? perfil.NombrePerfil : usuario.NombrePerfil,
                email = usuario.Email,
                activo = usuario.Activo,
                estadoCuenta = usuario.EstadoCuenta,
                fechaRegistro = usuario.FechaRegistro,
                ultimoLogin = usuario.UltimoLogin,
                roles = _context.UsuarioRoles
                    .Where(ur => ur.IdUsuario == usuario.IdUsuario)
                    .Select(ur => ur.Rol!.Nombre)
                    .OrderBy(nombre => nombre)
                    .ToList()
            };

        var total = await query.CountAsync(ct);
        var items = await query.Skip((pagina - 1) * cantidad).Take(cantidad).ToListAsync(ct);

        return Ok(new
        {
            exito = true,
            datos = new
            {
                items,
                pagina,
                total,
                totalPaginas = (int)Math.Ceiling(total / (double)cantidad),
                cantidad
            }
        });
    }

    [HttpPost("usuarios/{idUsuario:int}/accion")]
    public async Task<IActionResult> AccionUsuario(int idUsuario, [FromBody] AdminUsuarioAccionRequest request, CancellationToken ct)
    {
        var idAdmin = ObtenerIdUsuario();
        if (idAdmin is null) return Unauthorized();

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);
        if (usuario is null) return NotFound(new { exito = false, mensaje = "Usuario no encontrado." });

        var accion = request.Accion.Trim().ToLowerInvariant();
        if (accion is "desactivar" or "eliminar")
        {
            usuario.Activo = false;
            usuario.EstadoCuenta = accion == "eliminar" ? "eliminada" : "desactivada";
            usuario.MotivoEstadoCuenta = request.Motivo ?? $"Acción administrativa: {accion}";
            if (accion == "desactivar") usuario.FechaDesactivacion = DateTime.UtcNow;
            if (accion == "eliminar") usuario.FechaEliminacion = DateTime.UtcNow;
        }
        else if (accion is "moderador" or "admin")
        {
            var rol = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == accion, ct);
            if (rol is null) return BadRequest(new { exito = false, mensaje = "Rol no disponible." });

            var existe = await _context.UsuarioRoles.AnyAsync(ur => ur.IdUsuario == idUsuario && ur.IdRol == rol.IdRol, ct);
            if (!existe)
            {
                await _context.UsuarioRoles.AddAsync(new UsuarioRol
                {
                    IdUsuario = idUsuario,
                    IdRol = rol.IdRol,
                    AsignadoPor = idAdmin.Value,
                    FechaAsignacion = DateTime.UtcNow
                }, ct);
            }
        }
        else
        {
            return BadRequest(new { exito = false, mensaje = "Acción no válida." });
        }

        await _context.SaveChangesAsync(ct);
        return Ok(new { exito = true, mensaje = "Acción administrativa registrada." });
    }

    private int? ObtenerIdUsuario()
    {
        var idClaim = User.FindFirst("sub")?.Value;
        return int.TryParse(idClaim, out var idUsuario) ? idUsuario : null;
    }
}

public class AdminUsuarioAccionRequest
{
    public string Accion { get; set; } = string.Empty;
    public string? Motivo { get; set; }
}
