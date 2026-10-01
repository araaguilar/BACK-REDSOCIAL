namespace RedSocial.Domain.Entities;

public class RecuperacionPassword
{
    public int IdRecuperacionPassword { get; set; }
    public int IdUsuario { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CodigoHash { get; set; } = string.Empty;
    public string TokenRecuperacion { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public int IntentosFallidos { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaVerificacion { get; set; }
    public bool Usado { get; set; }

    public Usuario? Usuario { get; set; }
}
