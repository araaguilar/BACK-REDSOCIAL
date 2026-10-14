using System.Security.Cryptography;
using System.Text;

namespace RedSocial.API.Services;

public static class ChatSecurity
{
    public static (int A, int B) OrdenarUsuarios(int uno, int dos) => uno < dos ? (uno, dos) : (dos, uno);

    public static string CrearHash(int usuarioA, int usuarioB, string key)
    {
        var (a, b) = OrdenarUsuarios(usuarioA, usuarioB);
        var secreto = string.IsNullOrWhiteSpace(key) ? "moment-dev-chat-hash" : key;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secreto));
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes($"conversation:{a}:{b}"));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
