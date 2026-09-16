using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Application.Interfaces.Security;
using RedSocial.Infrastructure.Persistence;
using RedSocial.Infrastructure.Repositories;
using RedSocial.Infrastructure.Security;

namespace RedSocial.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<SecuritySettings>(configuration.GetSection(SecuritySettings.Seccion));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.Seccion));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtGenerator, JwtGenerator>();

        return services;
    }
}
