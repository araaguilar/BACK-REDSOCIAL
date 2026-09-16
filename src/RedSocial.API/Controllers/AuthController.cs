using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<IActionResult> Registro([FromBody] RegistroRequestDto dto, CancellationToken ct)
    {
        var resultado = await _authService.RegistrarAsync(dto, ct);
        return resultado.Exito ? Ok(resultado) : Conflict(resultado);
    }

    // Endpoint protegido para comprobar que el JWT funciona.
    [HttpGet("perfil")]
    [Authorize]
    public IActionResult Perfil() => Ok(new
    {
        IdUsuario = User.FindFirst("sub")?.Value,
        NombreUsuario = User.Identity?.Name,
        Email = User.FindFirst("email")?.Value
    });
}
