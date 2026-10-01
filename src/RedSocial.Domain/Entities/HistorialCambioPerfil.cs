namespace RedSocial.Domain.Entities;

public class HistorialCambioPerfil
{
    public int IdCambio { get; set; }
    public int IdUsuario { get; set; }
    public string TipoCambio { get; set; } = string.Empty;
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

    public Usuario? Usuario { get; set; }
}
