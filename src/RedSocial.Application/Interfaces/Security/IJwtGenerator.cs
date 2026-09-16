using RedSocial.Domain.Entities;

namespace RedSocial.Application.Interfaces.Security;

public interface IJwtGenerator
{
    (string Token, DateTime ExpiraEn) Generar(Usuario usuario);
}
