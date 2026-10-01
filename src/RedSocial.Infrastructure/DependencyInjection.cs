using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RedSocial.Application.Interfaces.Persistence;
using RedSocial.Application.Interfaces.Security;
using RedSocial.Application.Interfaces.Services;
using RedSocial.Infrastructure.Email;
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
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.Seccion));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPerfilUsuarioRepository, PerfilUsuarioRepository>();
        services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtGenerator, JwtGenerator>();

        return services;
    }
}
