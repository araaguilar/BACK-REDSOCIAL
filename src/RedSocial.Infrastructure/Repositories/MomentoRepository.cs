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

    public Task<List<MomentoFeedDto>> ObtenerFeedAsync(int cantidad = 30, CancellationToken ct = default)
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
                FechaCreacion = momento.FechaCreacion
            }).Take(take).ToListAsync(ct);
    }

    public async Task<MomentosPaginadosDto> ObtenerPorUsuarioAsync(int idUsuario, int? cursor, int cantidad = 30, CancellationToken ct = default)
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

    public Task GuardarCambiosAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
