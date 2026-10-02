using RedSocial.Application.DTOs.Momentos;

namespace RedSocial.Application.DTOs.Busqueda;

public class ResultadoBusquedaDto
{
    public List<PerfilBusquedaDto> Perfiles { get; set; } = [];
    public List<MomentoFeedDto> Momentos { get; set; } = [];
}
