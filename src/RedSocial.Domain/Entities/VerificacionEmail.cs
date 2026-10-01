namespace RedSocial.Domain.Entities;

public class VerificacionEmail
{
    public int IdVerificacionEmail { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CodigoHash { get; set; } = string.Empty;
    public string TokenVerificacion { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public int IntentosFallidos { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaVerificacion { get; set; }
    public bool Usado { get; set; }
}
