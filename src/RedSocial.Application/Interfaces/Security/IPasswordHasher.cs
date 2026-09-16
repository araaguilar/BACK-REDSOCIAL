namespace RedSocial.Application.Interfaces.Security;

// Application define el contrato; Infrastructure decide el algoritmo (BCrypt).
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verificar(string password, string hash);
}
