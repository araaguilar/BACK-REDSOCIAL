using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Domain.Entities;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.Infrastructure.Repositories;

public class PerfilUsuarioRepository : IPerfilUsuarioRepository
{
    private readonly AppDbContext _context;

    public PerfilUsuarioRepository(AppDbContext context) => _context = context;

    public async Task AgregarAsync(PerfilUsuario perfil, CancellationToken ct = default) =>
        await _context.PerfilesUsuario.AddAsync(perfil, ct);
}
