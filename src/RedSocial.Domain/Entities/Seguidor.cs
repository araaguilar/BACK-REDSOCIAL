namespace RedSocial.Domain.Entities;

public class Seguidor
{
    public int IdSeguidorRelacion { get; set; }
    public int IdSeguidor { get; set; }
    public int IdSeguido { get; set; }
    public DateTime FechaSeguimiento { get; set; } = DateTime.UtcNow;
}
