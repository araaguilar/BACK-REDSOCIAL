using Microsoft.EntityFrameworkCore;
using RedSocial.Application.DTOs.Perfil;
using RedSocial.Application.Interfaces.Persistence;
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
}
