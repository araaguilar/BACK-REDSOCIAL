using Microsoft.EntityFrameworkCore;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Domain.Entities;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.Infrastructure.Repositories;

public class RecuperacionPasswordRepository : IRecuperacionPasswordRepository
{
    private readonly AppDbContext _context;

    public RecuperacionPasswordRepository(AppDbContext context) => _context = context;

    public Task<RecuperacionPassword?> ObtenerActivaPorEmailAsync(string email, CancellationToken ct = default) =>
        _context.RecuperacionesPassword
            .Where(r => r.Email == email && !r.Usado && r.FechaVerificacion == null && r.ExpiraEn > DateTime.UtcNow)
            .OrderByDescending(r => r.FechaCreacion)
            .FirstOrDefaultAsync(ct);

    public Task<RecuperacionPassword?> ObtenerVerificadaPorTokenAsync(string token, CancellationToken ct = default) =>
        _context.RecuperacionesPassword
            .FirstOrDefaultAsync(r => r.TokenRecuperacion == token && !r.Usado && r.FechaVerificacion != null && r.ExpiraEn > DateTime.UtcNow, ct);

    public async Task InvalidarPendientesAsync(string email, CancellationToken ct = default)
    {
        var pendientes = await _context.RecuperacionesPassword
            .Where(r => r.Email == email && !r.Usado)
            .ToListAsync(ct);

        foreach (var pendiente in pendientes)
        {
            pendiente.Usado = true;
        }
    }

    public async Task AgregarAsync(RecuperacionPassword recuperacion, CancellationToken ct = default) =>
        await _context.RecuperacionesPassword.AddAsync(recuperacion, ct);

    public Task GuardarCambiosAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
