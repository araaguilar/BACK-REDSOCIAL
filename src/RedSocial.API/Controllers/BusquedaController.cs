using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedSocial.Application.Interfaces.Services;

namespace RedSocial.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusquedaController : ControllerBase
{
    private readonly IBusquedaService _busquedaService;

    public BusquedaController(IBusquedaService busquedaService) => _busquedaService = busquedaService;

    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] string? q, CancellationToken ct)
    {
        var idClaim = User.FindFirst("sub")?.Value;
        if (!int.TryParse(idClaim, out var idUsuario)) return Unauthorized();

        var resultado = await _busquedaService.BuscarAsync(q, idUsuario, ct);
        return Ok(resultado);
    }
}
