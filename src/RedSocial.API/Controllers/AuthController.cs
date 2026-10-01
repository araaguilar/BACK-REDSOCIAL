using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RedSocial.Application.DTOs.Auth;
using RedSocial.Application.Interfaces.Services;

namespace RedSocial.API.Controllers;

// El controller es delgado: solo traduce HTTP <-> caso de uso.
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var resultado = await _authService.LoginAsync(dto, ct);
        return resultado.Exito ? Ok(resultado) : Unauthorized(resultado);
    }

    [HttpPost("registro/solicitar-codigo")]
    [AllowAnonymous]
    [EnableRateLimiting("Registro")]
    public async Task<IActionResult> SolicitarCodigo([FromBody] SolicitarCodigoEmailRequestDto dto, CancellationToken ct)
    {
        var resultado = await _authService.SolicitarCodigoEmailAsync(dto, ct);
        return resultado.Exito ? Ok(resultado) : Conflict(resultado);
    }

    [HttpPost("registro/verificar-codigo")]
    [AllowAnonymous]
    [EnableRateLimiting("Registro")]
    public async Task<IActionResult> VerificarCodigo([FromBody] VerificarCodigoEmailRequestDto dto, CancellationToken ct)
    {
        var resultado = await _authService.VerificarCodigoEmailAsync(dto, ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    [HttpPost("registro/cancelar-codigo")]
    [AllowAnonymous]
    [EnableRateLimiting("Registro")]
    public async Task<IActionResult> CancelarCodigo([FromBody] SolicitarCodigoEmailRequestDto dto, CancellationToken ct)
    {
        var resultado = await _authService.CancelarCodigoEmailAsync(dto, ct);
        return Ok(resultado);
    }

    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<IActionResult> Registro([FromBody] RegistroRequestDto dto, CancellationToken ct)
    {
        var resultado = await _authService.RegistrarAsync(dto, ct);
        return resultado.Exito ? Ok(resultado) : Conflict(resultado);
    }

    [HttpGet("registro/usuario-disponible/{nombreUsuario}")]
    [AllowAnonymous]
    public async Task<IActionResult> UsuarioDisponible([FromRoute] string nombreUsuario, CancellationToken ct)
    {
        var resultado = await _authService.VerificarDisponibilidadUsuarioAsync(nombreUsuario, ct);
        return resultado.Exito ? Ok(resultado) : BadRequest(resultado);
    }

    // Endpoint protegido para comprobar que el JWT funciona.
    [HttpGet("perfil")]
    [Authorize]
    public IActionResult Perfil() => Ok(new
    {
        IdUsuario = User.FindFirst("sub")?.Value,
        NombreUsuario = User.Identity?.Name,
        Email = User.FindFirst("email")?.Value,
        Rol = User.FindFirst("rol")?.Value
    });
}
