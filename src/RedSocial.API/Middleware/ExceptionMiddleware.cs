using RedSocial.Application.Common;

namespace RedSocial.API.Middleware;

// Captura cualquier excepción no controlada: se registra el detalle en el log,
// pero al cliente solo le llega un mensaje genérico (no filtrar stack traces).
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado en {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(Resultado<object>.Error("Ocurrió un error inesperado. Intenta de nuevo."));
        }
    }
}
