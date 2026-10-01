namespace RedSocial.Application.DTOs.Momentos;

public class CrearMomentoDto
{
    public string Texto { get; set; } = string.Empty;
    public string? TipoAdjunto { get; set; }
    public string? ArchivoUrl { get; set; }
    public string? LinkUrl { get; set; }
}
