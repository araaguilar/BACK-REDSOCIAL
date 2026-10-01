using Microsoft.EntityFrameworkCore;
using RedSocial.Application.DTOs.Momentos;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Domain.Entities;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.Infrastructure.Repositories;

public class MomentoRepository : IMomentoRepository
{
    private readonly AppDbContext _context;

    public MomentoRepository(AppDbContext context) => _context = context;

    public async Task AgregarAsync(Momento momento, CancellationToken ct = default) =>
        await _context.Momentos.AddAsync(momento, ct);

    public Task<List<MomentoFeedDto>> ObtenerFeedAsync(int idUsuarioActual, int cantidad = 30, CancellationToken ct = default)
    {
        var take = Math.Clamp(cantidad, 1, 50);

        return (
            from momento in _context.Momentos.AsNoTracking()
            join usuario in _context.Usuarios.AsNoTracking() on momento.IdUsuario equals usuario.IdUsuario
            join perfil in _context.PerfilesUsuario.AsNoTracking() on usuario.IdUsuario equals perfil.IdUsuario into perfiles
            from perfil in perfiles.DefaultIfEmpty()
            where momento.Activo
            orderby momento.FechaCreacion descending
            select new MomentoFeedDto
            {
                IdMomento = momento.IdMomento,
                IdUsuario = usuario.IdUsuario,
                Autor = perfil != null ? perfil.NombrePerfil : usuario.NombrePerfil,
                Usuario = "@" + usuario.NombreUsuario,
                Avatar = perfil != null
                    ? perfil.NombrePerfil.Substring(0, Math.Min(perfil.NombrePerfil.Length, 2)).ToUpper()
                    : usuario.NombreUsuario.Substring(0, Math.Min(usuario.NombreUsuario.Length, 2)).ToUpper(),
                Texto = momento.Texto,
                TipoAdjunto = momento.TipoAdjunto,
                ArchivoUrl = momento.ArchivoUrl,
                LinkUrl = momento.LinkUrl,
                TotalMeGusta = momento.TotalMeGusta,
                TotalComentarios = momento.TotalComentarios,
                LeGusta = _context.MeGustaMomentos.Any(mg => mg.IdMomento == momento.IdMomento && mg.IdUsuario == idUsuarioActual),
                FechaCreacion = momento.FechaCreacion
            }).Take(take).ToListAsync(ct);
    }

    public async Task<MomentosPaginadosDto> ObtenerPorUsuarioAsync(int idUsuario, int idUsuarioActual, int? cursor, int cantidad = 30, CancellationToken ct = default)
    {
        var take = Math.Clamp(cantidad, 1, 30);

        var query =
            from momento in _context.Momentos.AsNoTracking()
            join usuario in _context.Usuarios.AsNoTracking() on momento.IdUsuario equals usuario.IdUsuario
            join perfil in _context.PerfilesUsuario.AsNoTracking() on usuario.IdUsuario equals perfil.IdUsuario into perfiles
            from perfil in perfiles.DefaultIfEmpty()
            where momento.Activo && momento.IdUsuario == idUsuario && (!cursor.HasValue || momento.IdMomento < cursor.Value)
            orderby momento.IdMomento descending
            select new MomentoFeedDto
            {
                IdMomento = momento.IdMomento,
                IdUsuario = usuario.IdUsuario,
                Autor = perfil != null ? perfil.NombrePerfil : usuario.NombrePerfil,
                Usuario = "@" + usuario.NombreUsuario,
                Avatar = perfil != null
                    ? perfil.NombrePerfil.Substring(0, Math.Min(perfil.NombrePerfil.Length, 2)).ToUpper()
                    : usuario.NombreUsuario.Substring(0, Math.Min(usuario.NombreUsuario.Length, 2)).ToUpper(),
                Texto = momento.Texto,
                TipoAdjunto = momento.TipoAdjunto,
                ArchivoUrl = momento.ArchivoUrl,
                LinkUrl = momento.LinkUrl,
                TotalMeGusta = momento.TotalMeGusta,
                TotalComentarios = momento.TotalComentarios,
                LeGusta = _context.MeGustaMomentos.Any(mg => mg.IdMomento == momento.IdMomento && mg.IdUsuario == idUsuarioActual),
                FechaCreacion = momento.FechaCreacion
            };

        var items = await query.Take(take + 1).ToListAsync(ct);
        var tieneMas = items.Count > take;
        if (tieneMas) items.RemoveAt(items.Count - 1);

        var total = await _context.Momentos
            .AsNoTracking()
            .CountAsync(m => m.Activo && m.IdUsuario == idUsuario, ct);

        return new MomentosPaginadosDto
        {
            Items = items,
            TieneMas = tieneMas,
            SiguienteCursor = tieneMas ? items.LastOrDefault()?.IdMomento : null,
            Total = total
        };
    }

    public Task<MomentoFeedDto?> ObtenerPorIdAsync(int idMomento, int idUsuarioActual, CancellationToken ct = default)
    {
        return (
            from momento in _context.Momentos.AsNoTracking()
            join usuario in _context.Usuarios.AsNoTracking() on momento.IdUsuario equals usuario.IdUsuario
            join perfil in _context.PerfilesUsuario.AsNoTracking() on usuario.IdUsuario equals perfil.IdUsuario into perfiles
            from perfil in perfiles.DefaultIfEmpty()
            where momento.Activo && momento.IdMomento == idMomento
            select new MomentoFeedDto
            {
                IdMomento = momento.IdMomento,
                IdUsuario = usuario.IdUsuario,
                Autor = perfil != null ? perfil.NombrePerfil : usuario.NombrePerfil,
                Usuario = "@" + usuario.NombreUsuario,
                Avatar = perfil != null
                    ? perfil.NombrePerfil.Substring(0, Math.Min(perfil.NombrePerfil.Length, 2)).ToUpper()
                    : usuario.NombreUsuario.Substring(0, Math.Min(usuario.NombreUsuario.Length, 2)).ToUpper(),
                Texto = momento.Texto,
                TipoAdjunto = momento.TipoAdjunto,
                ArchivoUrl = momento.ArchivoUrl,
                LinkUrl = momento.LinkUrl,
                TotalMeGusta = momento.TotalMeGusta,
                TotalComentarios = momento.TotalComentarios,
                LeGusta = _context.MeGustaMomentos.Any(mg => mg.IdMomento == momento.IdMomento && mg.IdUsuario == idUsuarioActual),
                FechaCreacion = momento.FechaCreacion
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<MeGustaMomentoDto?> AlternarMeGustaAsync(int idMomento, int idUsuario, CancellationToken ct = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        var momento = await _context.Momentos.FirstOrDefaultAsync(m => m.IdMomento == idMomento && m.Activo, ct);
        if (momento is null) return null;

        var existente = await _context.MeGustaMomentos
            .FirstOrDefaultAsync(mg => mg.IdMomento == idMomento && mg.IdUsuario == idUsuario, ct);

        var leGusta = existente is null;
        var cambio = leGusta ? 1 : -1;

        if (leGusta)
        {
            await _context.MeGustaMomentos.AddAsync(new MomentoMeGusta
            {
                IdMomento = idMomento,
                IdUsuario = idUsuario,
                FechaCreacion = DateTime.UtcNow
            }, ct);
        }
        else
        {
            _context.MeGustaMomentos.Remove(existente!);
        }

        momento.TotalMeGusta = Math.Max(0, momento.TotalMeGusta + cambio);

        var perfilAutor = await _context.PerfilesUsuario.FirstOrDefaultAsync(p => p.IdUsuario == momento.IdUsuario, ct);
        if (perfilAutor is not null)
            perfilAutor.TotalMeEncanta = Math.Max(0, perfilAutor.TotalMeEncanta + cambio);

        await _context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new MeGustaMomentoDto
        {
            IdMomento = idMomento,
            LeGusta = leGusta,
            TotalMeGusta = momento.TotalMeGusta
        };
    }

    public Task GuardarCambiosAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
