using Microsoft.EntityFrameworkCore;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<LogAcceso> LogsAcceso => Set<LogAcceso>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<PerfilUsuario> PerfilesUsuario => Set<PerfilUsuario>();
    public DbSet<Seguidor> Seguidores => Set<Seguidor>();
    public DbSet<Momento> Momentos => Set<Momento>();
    public DbSet<Reporte> Reportes => Set<Reporte>();
    public DbSet<ModeracionAccion> ModeracionAcciones => Set<ModeracionAccion>();
    public DbSet<AdvertenciaUsuario> AdvertenciasUsuario => Set<AdvertenciaUsuario>();
    public DbSet<MomentoMeGusta> MeGustaMomentos => Set<MomentoMeGusta>();
    public DbSet<HistorialCambioPerfil> HistorialCambiosPerfil => Set<HistorialCambioPerfil>();
    public DbSet<VerificacionEmail> VerificacionesEmail => Set<VerificacionEmail>();
    public DbSet<RecuperacionPassword> RecuperacionesPassword => Set<RecuperacionPassword>();
    public DbSet<Conversacion> Conversaciones => Set<Conversacion>();
    public DbSet<MensajeChat> MensajesChat => Set<MensajeChat>();
    public DbSet<ComentarioMomento> ComentariosMomentos => Set<ComentarioMomento>();
    public DbSet<ComentarioMomentoMeGusta> MeGustaComentariosMomentos => Set<ComentarioMomentoMeGusta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
