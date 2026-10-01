using Microsoft.EntityFrameworkCore;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Domain.Entities;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.Infrastructure.Repositories;

public class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly AppDbContext _context;

    public EmailVerificationRepository(AppDbContext context) => _context = context;

    public Task<VerificacionEmail?> ObtenerActivaPorEmailAsync(string email, CancellationToken ct = default) =>
        _context.VerificacionesEmail
            .Where(v => v.Email == email && !v.Usado && v.FechaVerificacion == null && v.ExpiraEn > DateTime.UtcNow)
            .OrderByDescending(v => v.FechaCreacion)
            .FirstOrDefaultAsync(ct);

    public Task<VerificacionEmail?> ObtenerVerificadaPorTokenAsync(string token, CancellationToken ct = default) =>
        _context.VerificacionesEmail
            .FirstOrDefaultAsync(v => v.TokenVerificacion == token && !v.Usado && v.FechaVerificacion != null && v.ExpiraEn > DateTime.UtcNow, ct);

    public async Task InvalidarPendientesAsync(string email, CancellationToken ct = default)
    {
        var pendientes = await _context.VerificacionesEmail
            .Where(v => v.Email == email && !v.Usado)
            .ToListAsync(ct);

        foreach (var pendiente in pendientes)
        {
            pendiente.Usado = true;
        }
    }

    public async Task EliminarPendientesAsync(string email, CancellationToken ct = default)
    {
        var pendientes = await _context.VerificacionesEmail
            .Where(v => v.Email == email && !v.Usado)
            .ToListAsync(ct);

        _context.VerificacionesEmail.RemoveRange(pendientes);
    }

    public async Task AgregarAsync(VerificacionEmail verificacion, CancellationToken ct = default) =>
        await _context.VerificacionesEmail.AddAsync(verificacion, ct);

    public Task GuardarCambiosAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
