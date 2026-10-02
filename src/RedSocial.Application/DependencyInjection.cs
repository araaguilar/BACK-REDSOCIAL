using Microsoft.Extensions.DependencyInjection;
using RedSocial.Application.Interfaces.Services;
using RedSocial.Application.Services;

namespace RedSocial.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPerfilService, PerfilService>();
        services.AddScoped<IMomentoService, MomentoService>();
        services.AddScoped<IBusquedaService, BusquedaService>();
        return services;
    }
}
