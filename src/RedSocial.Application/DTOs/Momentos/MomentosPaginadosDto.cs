namespace RedSocial.Application.DTOs.Momentos;

public class MomentosPaginadosDto
{
    public List<MomentoFeedDto> Items { get; set; } = [];
    public int? SiguienteCursor { get; set; }
    public bool TieneMas { get; set; }
    public int Total { get; set; }
}
