using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RedSocial.API.Services;
using RedSocial.Domain.Entities;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _context;
    private readonly PresenceTracker _presence;

    public ChatHub(AppDbContext context, PresenceTracker presence)
    {
        _context = context;
        _presence = presence;
    }

    public override async Task OnConnectedAsync()
    {
        var idUsuario = ObtenerIdUsuario();
        if (idUsuario is not null)
        {
            _presence.Conectar(idUsuario.Value, Context.ConnectionId);
            await Clients.All.SendAsync("UsuariosEnLinea", _presence.EnLinea());
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var idUsuario = ObtenerIdUsuario();
        if (idUsuario is not null)
        {
            _presence.Desconectar(idUsuario.Value, Context.ConnectionId);
            await Clients.All.SendAsync("UsuariosEnLinea", _presence.EnLinea());
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task UnirseConversacion(string hash)
    {
        var idUsuario = ObtenerIdUsuario();
        if (idUsuario is null) return;

        var permitido = await _context.Conversaciones.AsNoTracking()
            .AnyAsync(c => c.HashConversacion == hash && (c.IdUsuarioA == idUsuario || c.IdUsuarioB == idUsuario));
        if (!permitido) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, hash);
    }

    public async Task EnviarMensaje(string hash, string texto)
    {
        var idUsuario = ObtenerIdUsuario();
        if (idUsuario is null) return;

        texto = (texto ?? string.Empty).Trim();
        if (texto.Length is < 1 or > 1000) return;

        var conversacion = await _context.Conversaciones
            .FirstOrDefaultAsync(c => c.HashConversacion == hash && (c.IdUsuarioA == idUsuario || c.IdUsuarioB == idUsuario));
        if (conversacion is null) return;

        await Groups.AddToGroupAsync(Context.ConnectionId, hash);

        var mensaje = new MensajeChat
        {
            IdConversacion = conversacion.IdConversacion,
            IdEmisor = idUsuario.Value,
            Texto = texto,
            FechaEnvio = DateTime.UtcNow
        };

        conversacion.FechaActualizacion = mensaje.FechaEnvio;
        await _context.MensajesChat.AddAsync(mensaje);
        await _context.SaveChangesAsync();

        await Clients.Group(hash).SendAsync("MensajeRecibido", new
        {
            idMensaje = mensaje.IdMensaje,
            hash,
            idEmisor = mensaje.IdEmisor,
            texto = mensaje.Texto,
            fechaEnvio = mensaje.FechaEnvio
        });
    }

    private int? ObtenerIdUsuario()
    {
        var idClaim = Context.User?.FindFirst("sub")?.Value;
        return int.TryParse(idClaim, out var idUsuario) ? idUsuario : null;
    }
}
