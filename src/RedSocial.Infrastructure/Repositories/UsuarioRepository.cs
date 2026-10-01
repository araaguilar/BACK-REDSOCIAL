using Microsoft.EntityFrameworkCore;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Domain.Entities;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context) => _context = context;

    // EF parametriza las consultas LINQ, por lo que no hay riesgo de SQL Injection.
    public Task<Usuario?> ObtenerPorUsuarioOEmailAsync(string usuarioOEmail, CancellationToken ct = default)
    {
        var email = usuarioOEmail.ToLowerInvariant();
        return _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == usuarioOEmail || u.Email == email, ct);
    }

    public Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken ct = default) =>
        _context.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario, ct);

    public Task<bool> ExisteNombreUsuarioEnLookupAsync(string nombreUsuario, CancellationToken ct = default) =>
        _context.Database
            .SqlQuery<int>($"SELECT 1 AS Value FROM dbo.vw_UsuariosLookup WHERE NombreUsuario = {nombreUsuario}")
            .AnyAsync(ct);

    public Task<bool> ExisteEmailAsync(string email, CancellationToken ct = default) =>
        _context.Usuarios.AnyAsync(u => u.Email == email, ct);

    public Task<bool> ExisteEmailEnLookupAsync(string email, CancellationToken ct = default) =>
        _context.Database
            .SqlQuery<int>($"SELECT 1 AS Value FROM dbo.vw_EmailsLookup WHERE Email = {email}")
            .AnyAsync(ct);

    public async Task AgregarAsync(Usuario usuario, CancellationToken ct = default) =>
        await _context.Usuarios.AddAsync(usuario, ct);

    public Task GuardarCambiosAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
