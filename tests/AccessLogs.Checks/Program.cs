using Microsoft.AspNetCore.Builder;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RedSocial.API.Controllers;
using RedSocial.API.Services;
using RedSocial.Application.Common;
using RedSocial.Application.DTOs.Auth;
using RedSocial.Domain.Entities;
using RedSocial.Infrastructure.Persistence;

int comprobaciones = 0;
void Check(bool ok, string nombre)
{
    if (!ok) throw new Exception(nombre);
    Console.WriteLine("OK: " + nombre);
    comprobaciones++;
}
var respuesta = new AuthResponseDto { IdUsuario = 42, NombreUsuario = "ana_1", Token = "secreto" };
var antes = DateTimeOffset.UtcNow;
var log = AccessLogService.Crear("correo@ejemplo.com", "login", Resultado<AuthResponseDto>.Ok(respuesta), IPAddress.Parse("::ffff:192.0.2.1"));
Check(log.Exitoso && log.IdUsuario == 42 && log.Identificador == "ana_1", "Identidad confirmada en acceso exitoso");
Check(log.DireccionIp == "192.0.2.1", "Normalizacion IPv4 mapeada");
Check(log.FechaUtc.Offset == TimeSpan.Zero && log.FechaUtc >= antes && log.FechaUtc <= DateTimeOffset.UtcNow, "Fecha UTC del servidor");
var fallo = AccessLogService.Crear("  intento_1  ", "login", Resultado<AuthResponseDto>.Error("incorrecto"), IPAddress.Parse("2001:db8::1"));
Check(!fallo.Exitoso && fallo.IdUsuario == null && fallo.Identificador == "intento_1" && fallo.DireccionIp == "2001:db8::1", "Intento fallido e IPv6");
var inactivo = AccessLogService.Crear("ana_1", "login", Resultado<AuthResponseDto>.Error("Cuenta desactivada", respuesta), null);
Check(!inactivo.Exitoso && inactivo.IdUsuario == null && inactivo.DireccionIp == null, "Cuenta inactiva no cuenta como ingreso exitoso");
Check(AccessLogService.Crear("ana_1", "reactivacion", Resultado<AuthResponseDto>.Ok(respuesta), null).Evento == "reactivacion", "Evento de reactivacion");
Check(AccessLogService.Crear(new string('a', 120), "login", Resultado<AuthResponseDto>.Error("error"), null).Identificador.Length == 100, "Limite de identificador");
Check(!typeof(LogAcceso).GetProperties().Any(p => p.Name.Contains("Password") || p.Name.Contains("Token")), "Sin contrasenas ni tokens en entidad");
var permiso = (AuthorizeAttribute?)Attribute.GetCustomAttribute(typeof(AdminController), typeof(AuthorizeAttribute));
Check(permiso?.Roles == "admin", "Consulta restringida a administradores");
using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlServer("Server=unused;Database=unused;Trusted_Connection=True").Options);
var entidad = db.Model.FindEntityType(typeof(LogAcceso))!;
Check(entidad.GetTableName() == "LogsAcceso" && entidad.FindProperty("DireccionIp")!.GetMaxLength() == 45, "Mapeo SQL de tabla e IP");
Check(entidad.GetIndexes().Any(i => i.Properties.Select(p => p.Name).SequenceEqual(new[] { "FechaUtc", "IdLogAcceso" })), "Indice de paginacion");

async Task<string?> ProcesarProxy(string ipRemota)
{
    var options = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto, ForwardLimit = 1 };
    options.KnownNetworks.Clear(); options.KnownProxies.Clear(); options.KnownProxies.Add(IPAddress.Parse("10.0.0.5"));
    var http = new DefaultHttpContext();
    http.Connection.RemoteIpAddress = IPAddress.Parse(ipRemota);
    http.Request.Headers["X-Forwarded-For"] = "198.51.100.8";
    var middleware = new ForwardedHeadersMiddleware(_ => Task.CompletedTask, NullLoggerFactory.Instance, Options.Create(options));
    await middleware.Invoke(http);
    return http.Connection.RemoteIpAddress?.ToString();
}
Check(await ProcesarProxy("10.0.0.5") == "198.51.100.8", "Proxy autorizado conserva IP cliente");
Check(await ProcesarProxy("192.0.2.9") == "192.0.2.9", "Cabecera de origen no autorizado se ignora");
Console.WriteLine($"{comprobaciones} comprobaciones correctas.");
