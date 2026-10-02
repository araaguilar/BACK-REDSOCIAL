namespace RedSocial.Domain.Entities;

public class Reporte
{
    public int IdReporte { get; set; }
    public int IdReportante { get; set; }
    public int? IdUsuarioReportado { get; set; }
    public int? IdMomentoReportado { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public string Estado { get; set; } = "abierto";
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
