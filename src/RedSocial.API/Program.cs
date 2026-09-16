using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RedSocial.API.Middleware;
using RedSocial.Application;
using RedSocial.Infrastructure;
using RedSocial.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

// Secretos locales de desarrollo (cadena de conexión, clave JWT) en appsettings.Local.json.
// Ese archivo está en .gitignore: cada integrante crea el suyo a partir de appsettings.Local.example.json.
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// ── Capas ────────────────────────────────────────────────────────────────
builder.Services.AddApplication();  
builder.Services.AddInfrastructure(builder.Configuration);

// ── JWT ──────────────────────────────────────────────────────────────────
var jwt = builder.Configuration.GetSection(JwtSettings.Seccion).Get<JwtSettings>()!;
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
    throw new InvalidOperationException(
        $"Jwt:Key debe tener al menos 32 caracteres (entorno: '{builder.Environment.EnvironmentName}'). Configúrala en appsettings.Local.json.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false; // conservar nombres de claims ("sub", "email")
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            NameClaimType = JwtRegisteredClaimNames.UniqueName,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// ── CORS para el frontend React ──────────────────────────────────────────
var origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", p => p.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RedSocial API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Pega aquí el token JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, [] }
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // En producción se fuerza HTTPS. En desarrollo no: la redirección 307 rompe el
    // preflight CORS cuando el frontend llama a http://localhost:5079.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
