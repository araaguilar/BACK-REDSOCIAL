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
                Seguidores = _context.Seguidores.Count(s => s.IdSeguido == u.IdUsuario),
                Seguidos = _context.Seguidores.Count(s => s.IdSeguidor == u.IdUsuario),
                TotalMeEncanta = u.Perfil != null ? u.Perfil.TotalMeEncanta : 0
            })
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
}
