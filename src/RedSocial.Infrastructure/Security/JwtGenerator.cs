using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RedSocial.Application.Interfaces.Security;
using RedSocial.Domain.Entities;

namespace RedSocial.Infrastructure.Security;

public class JwtGenerator : IJwtGenerator
{
    private readonly JwtSettings _settings;

    public JwtGenerator(IOptions<JwtSettings> options) => _settings = options.Value;

    public (string Token, DateTime ExpiraEn) Generar(Usuario usuario)
    {
        var expira = DateTime.UtcNow.AddMinutes(_settings.ExpireMinutes);

        // Nunca incluir datos sensibles (hash, contraseña) en el token: el payload solo va en Base64.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, usuario.NombreUsuario),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expira,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
