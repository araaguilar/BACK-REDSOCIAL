namespace RedSocial.Application.DTOs.Momentos;

public class MomentoFeedDto
{
    public int IdMomento { get; set; }
    public int IdUsuario { get; set; }
    public string Autor { get; set; } = string.Empty;
    public string Usuario { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
    public string? TipoAdjunto { get; set; }
    public string? ArchivoUrl { get; set; }
    public string? LinkUrl { get; set; }
    public int TotalMeGusta { get; set; }
    public int TotalComentarios { get; set; }
    public bool LeGusta { get; set; }
    public DateTime FechaCreacion { get; set; }
}
