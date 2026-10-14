namespace RedSocial.Application.DTOs.Auth;

public class SolicitarCodigoEmailResponseDto
{
    public int ExpiraEnMinutos { get; set; }
    public bool CuentaDesactivada { get; set; }
    public DateTime? FechaDesactivacion { get; set; }
}
