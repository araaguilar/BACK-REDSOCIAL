namespace RedSocial.Application.DTOs.Auth;

public class SolicitarCodigoEmailResponseDto
{
    public int ExpiraEnMinutos { get; set; }
    public bool CuentaDesactivada { get; set; }
    public string EstadoCuenta { get; set; } = "activa";
    public DateTime? FechaDesactivacion { get; set; }
    public DateTime? FechaEliminacion { get; set; }
}
