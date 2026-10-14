using System.Collections.Concurrent;

namespace RedSocial.API.Services;

public class PresenceTracker
{
    private readonly ConcurrentDictionary<int, HashSet<string>> _connections = new();

    public bool Conectar(int idUsuario, string connectionId)
    {
        var conexiones = _connections.GetOrAdd(idUsuario, _ => []);
        lock (conexiones)
        {
            conexiones.Add(connectionId);
            return conexiones.Count == 1;
        }
    }

    public bool Desconectar(int idUsuario, string connectionId)
    {
        if (!_connections.TryGetValue(idUsuario, out var conexiones)) return false;

        lock (conexiones)
        {
            conexiones.Remove(connectionId);
            if (conexiones.Count > 0) return false;
        }

        _connections.TryRemove(idUsuario, out _);
        return true;
    }

    public bool EstaEnLinea(int idUsuario) => _connections.ContainsKey(idUsuario);

    public IReadOnlyCollection<int> EnLinea() => _connections.Keys.ToArray();
}
