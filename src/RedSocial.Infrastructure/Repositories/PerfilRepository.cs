using Microsoft.EntityFrameworkCore;
using RedSocial.Application.DTOs.Perfil;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Domain.Entities;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.Infrastructure.Repositories;

public class PerfilRepository : IPerfilRepository
{
    private readonly AppDbContext _context;

    public PerfilRepository(AppDbContext context) => _context = context;

    public Task<MiPerfilDto?> ObtenerMiPerfilAsync(int idUsuario, CancellationToken ct = default) =>
        _context.Usuarios
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => new MiPerfilDto
            {
                IdUsuario = u.IdUsuario,
                NombreUsuario = u.NombreUsuario,
                NombrePerfil = u.Perfil != null ? u.Perfil.NombrePerfil : u.NombrePerfil,
                SobreMi = u.Perfil != null ? u.Perfil.SobreMi : null,
                FotoPerfilUrl = u.Perfil != null ? u.Perfil.FotoPerfilUrl : null,
                Seguidores = _context.Seguidores.Count(s => s.IdSeguido == u.IdUsuario),
                Seguidos = _context.Seguidores.Count(s => s.IdSeguidor == u.IdUsuario),
                TotalMeEncanta = u.Perfil != null ? u.Perfil.TotalMeEncanta : 0,
                ProximoCambioNombrePerfil = _context.HistorialCambiosPerfil
                    .Where(h => h.IdUsuario == u.IdUsuario && h.TipoCambio == "nombre_perfil")
                    .OrderByDescending(h => h.FechaCambio)
                    .Select(h => (DateTime?)h.FechaCambio.AddDays(3))
                    .FirstOrDefault(),
                ProximoCambioNombreUsuario = _context.HistorialCambiosPerfil
                    .Where(h => h.IdUsuario == u.IdUsuario && h.TipoCambio == "nombre_usuario")
                    .OrderByDescending(h => h.FechaCambio)
                    .Select(h => (DateTime?)h.FechaCambio.AddDays(21))
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

    public Task<DateTime?> ObtenerUltimoCambioAsync(int idUsuario, string tipoCambio, CancellationToken ct = default) =>
        _context.HistorialCambiosPerfil
            .Where(h => h.IdUsuario == idUsuario && h.TipoCambio == tipoCambio)
            .OrderByDescending(h => h.FechaCambio)
            .Select(h => (DateTime?)h.FechaCambio)
            .FirstOrDefaultAsync(ct);

    public async Task<MiPerfilDto?> ActualizarSobreMiAsync(int idUsuario, string? sobreMi, CancellationToken ct = default)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);

        if (usuario is null) return null;

        if (usuario.Perfil is null)
        {
            usuario.Perfil = new PerfilUsuario
            {
                IdUsuario = usuario.IdUsuario,
                NombrePerfil = usuario.NombrePerfil,
                FechaNacimiento = usuario.FechaNacimiento,
                SobreMi = sobreMi,
                FechaCreacion = DateTime.UtcNow
            };
        }
        else
        {
            usuario.Perfil.SobreMi = sobreMi;
            usuario.Perfil.FechaActualizacion = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
        return await ObtenerMiPerfilAsync(idUsuario, ct);
    }

    public async Task<MiPerfilDto?> ActualizarFotoPerfilAsync(int idUsuario, string fotoPerfilUrl, CancellationToken ct = default)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);

        if (usuario is null) return null;

        if (usuario.Perfil is null)
        {
            usuario.Perfil = new PerfilUsuario
            {
                IdUsuario = usuario.IdUsuario,
                NombrePerfil = usuario.NombrePerfil,
                FechaNacimiento = usuario.FechaNacimiento,
                FotoPerfilUrl = fotoPerfilUrl,
                FechaCreacion = DateTime.UtcNow
            };
        }
        else
        {
            usuario.Perfil.FotoPerfilUrl = fotoPerfilUrl;
            usuario.Perfil.FechaActualizacion = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
        return await ObtenerMiPerfilAsync(idUsuario, ct);
    }

    public async Task<MiPerfilDto?> ActualizarNombrePerfilAsync(int idUsuario, string nombrePerfil, CancellationToken ct = default)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);

        if (usuario is null) return null;

        var anterior = usuario.Perfil?.NombrePerfil ?? usuario.NombrePerfil;
        usuario.NombrePerfil = nombrePerfil;

        if (usuario.Perfil is null)
        {
            usuario.Perfil = new PerfilUsuario
            {
                IdUsuario = usuario.IdUsuario,
                NombrePerfil = nombrePerfil,
                FechaNacimiento = usuario.FechaNacimiento,
                FechaCreacion = DateTime.UtcNow
            };
        }
        else
        {
            usuario.Perfil.NombrePerfil = nombrePerfil;
            usuario.Perfil.FechaActualizacion = DateTime.UtcNow;
        }

        await _context.HistorialCambiosPerfil.AddAsync(new HistorialCambioPerfil
        {
            IdUsuario = idUsuario,
            TipoCambio = "nombre_perfil",
            ValorAnterior = anterior,
            ValorNuevo = nombrePerfil,
            FechaCambio = DateTime.UtcNow
        }, ct);

        await _context.SaveChangesAsync(ct);
        return await ObtenerMiPerfilAsync(idUsuario, ct);
    }

    public async Task<MiPerfilDto?> ActualizarNombreUsuarioAsync(int idUsuario, string nombreUsuario, CancellationToken ct = default)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Perfil)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);

        if (usuario is null) return null;

        var anterior = usuario.NombreUsuario;
        usuario.NombreUsuario = nombreUsuario;

        await _context.HistorialCambiosPerfil.AddAsync(new HistorialCambioPerfil
        {
            IdUsuario = idUsuario,
            TipoCambio = "nombre_usuario",
            ValorAnterior = anterior,
            ValorNuevo = nombreUsuario,
            FechaCambio = DateTime.UtcNow
        }, ct);

        await _context.SaveChangesAsync(ct);
        return await ObtenerMiPerfilAsync(idUsuario, ct);
    }
}
