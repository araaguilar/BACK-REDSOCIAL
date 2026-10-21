using System.Net;
using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Auth;
using RedSocial.Domain.Entities;
using RedSocial.Infrastructure.Persistence;

namespace RedSocial.API.Services;

public class AccessLogService(AppDbContext context)
{
    public async Task RegistrarAsync(string identificador, string evento,
        Resultado<AuthResponseDto> resultado, IPAddress? ip, CancellationToken ct)
    {
        context.LogsAcceso.Add(Crear(identificador, evento, resultado, ip));
        await context.SaveChangesAsync(ct);
    }

    public static LogAcceso Crear(string identificador, string evento,
        Resultado<AuthResponseDto> resultado, IPAddress? ip)
    {
        var confirmado = resultado.Exito && resultado.Datos is not null;
        var nombre = confirmado ? resultado.Datos!.NombreUsuario : identificador.Trim();
        return new LogAcceso
        {
            IdUsuario = confirmado ? resultado.Datos!.IdUsuario : null,
            Identificador = nombre[..Math.Min(nombre.Length, 100)],
            Evento = evento,
            Exitoso = confirmado,
            FechaUtc = DateTimeOffset.UtcNow,
            DireccionIp = ip?.IsIPv4MappedToIPv6 == true ? ip.MapToIPv4().ToString() : ip?.ToString()
        };
    }
}
