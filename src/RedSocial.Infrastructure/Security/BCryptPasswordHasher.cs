using Microsoft.Extensions.Options;
using RedSocial.Application.Interfaces.Security;
using BC = BCrypt.Net.BCrypt;

namespace RedSocial.Infrastructure.Security;

public class BCryptPasswordHasher : IPasswordHasher
{
    private readonly int _workFactor;

    public BCryptPasswordHasher(IOptions<SecuritySettings> options)
    {
        // Work factor = 2^N iteraciones. 12 es un buen equilibrio actual entre
        // seguridad y tiempo (~250 ms). Cada +1 duplica el costo para un atacante.
        _workFactor = options.Value.BCryptWorkFactor;
    }

    // Cada llamada genera un salt aleatorio distinto, así dos usuarios con la misma
    // contraseña tienen hashes diferentes (inutiliza las rainbow tables).
    public string Hash(string password) => BC.HashPassword(password, _workFactor);

    // Verify extrae el salt y el work factor del propio hash y compara en tiempo constante.
    public bool Verificar(string password, string hash)
    {
        if (string.IsNullOrEmpty(hash)) return false;
        try
        {
            return BC.Verify(password, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false; // hash corrupto o que no es BCrypt
        }
    }
}
