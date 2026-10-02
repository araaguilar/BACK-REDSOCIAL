using Microsoft.EntityFrameworkCore;
using RedSocial.Application.DTOs.Busqueda;
using RedSocial.Application.DTOs.Momentos;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.Infrastructure.Repositories;

public class BusquedaRepository : IBusquedaRepository
{
    private readonly AppDbContext _context;

    public BusquedaRepository(AppDbContext context) => _context = context;

    public Task<List<PerfilBusquedaDto>> BuscarPerfilesAsync(string termino, int cantidad = 12, CancellationToken ct = default)
    {
        var take = Math.Clamp(cantidad, 1, 30);
        var q = termino.ToLowerInvariant();

        return _context.Usuarios
            .AsNoTracking()
            .Where(u => u.Activo)
            .Select(u => new
            {
                u.IdUsuario,
                u.NombreUsuario,
                NombrePerfil = u.Perfil != null ? u.Perfil.NombrePerfil : u.NombrePerfil,
                SobreMi = u.Perfil != null ? u.Perfil.SobreMi : null,
                FotoPerfilUrl = u.Perfil != null ? u.Perfil.FotoPerfilUrl : null
            })
            .Where(u => q == string.Empty || u.NombreUsuario.Contains(q) || u.NombrePerfil.Contains(q))
            .OrderByDescending(u => q != string.Empty && u.NombreUsuario.StartsWith(q))
            .ThenByDescending(u => q != string.Empty && u.NombrePerfil.StartsWith(q))
            .ThenBy(u => u.NombreUsuario)
            .Take(take)
            .Select(u => new PerfilBusquedaDto
            {
                IdUsuario = u.IdUsuario,
                NombreUsuario = u.NombreUsuario,
                NombrePerfil = u.NombrePerfil,
                SobreMi = u.SobreMi,
                FotoPerfilUrl = u.FotoPerfilUrl
            })
            .ToListAsync(ct);
    }

    public Task<List<MomentoFeedDto>> BuscarMomentosAsync(string termino, int idUsuarioActual, int cantidad = 18, CancellationToken ct = default)
    {
        var take = Math.Clamp(cantidad, 1, 30);
        var q = termino.ToLowerInvariant();

        return (
            from momento in _context.Momentos.AsNoTracking()
            join usuario in _context.Usuarios.AsNoTracking() on momento.IdUsuario equals usuario.IdUsuario
            join perfil in _context.PerfilesUsuario.AsNoTracking() on usuario.IdUsuario equals perfil.IdUsuario into perfiles
            from perfil in perfiles.DefaultIfEmpty()
            where momento.Activo && usuario.Activo
                && (q == string.Empty
                    || momento.Texto.Contains(q)
                    || usuario.NombreUsuario.Contains(q)
                    || (perfil != null && perfil.NombrePerfil.Contains(q)))
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
}
