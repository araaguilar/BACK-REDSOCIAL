namespace RedSocial.Infrastructure.Security;

public class SecuritySettings
{
    public const string Seccion = "Security";
    public int BCryptWorkFactor { get; set; } = 12;
}

public class JwtSettings
{
    public const string Seccion = "Jwt";
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpireMinutes { get; set; } = 60;
}
