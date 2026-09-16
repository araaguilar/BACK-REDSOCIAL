# RedSocial — Backend

API REST en .NET 8 con Arquitectura Limpia. Tema actual: **login y hash de contraseñas con BCrypt**.

## Capas

```
src/
├── RedSocial.Domain          Entidades y reglas de negocio puras (sin dependencias)
├── RedSocial.Application     Casos de uso (AuthService), DTOs e interfaces (contratos)
├── RedSocial.Infrastructure  EF Core + SQL Server, BCrypt, JWT (implementa las interfaces)
└── RedSocial.API             Controllers, middleware, configuración
```

Dependencias: `API → Infrastructure → Application → Domain`. Application no sabe que existen EF ni BCrypt.

## Seguridad implementada

| Medida | Dónde |
|---|---|
| Hash BCrypt con salt aleatorio, work factor 12 | `Infrastructure/Security/BCryptPasswordHasher.cs` |
| Mensaje genérico en login (anti-enumeración) | `Application/Services/AuthService.cs` |
| Hash aunque el usuario no exista (anti-timing) | `AuthService.LoginAsync` |
| JWT firmado HS256, validación de expiración | `Infrastructure/Security/JwtGenerator.cs` |
| Secretos fuera del repo (user-secrets) | — |
| Errores sin stack trace al cliente | `API/Middleware/ExceptionMiddleware.cs` |

## Cómo correrlo

Requisitos: .NET 8 SDK y un SQL Server (local o en Docker).

1. Ejecutar `database/01_CrearBaseDatos.sql` en tu SQL Server (desde SSMS).
2. Crear tu configuración local (una sola vez por máquina) y llenarla con los datos de **tu** servidor y una clave JWT de 32+ caracteres:

```bash
cp src/RedSocial.API/appsettings.Local.example.json src/RedSocial.API/appsettings.Local.json
```

`appsettings.Local.json` está en `.gitignore`: nunca se sube al repo.

Cada integrante usa su propio servidor y contraseña; nada de eso se sube al repo. Los scripts de `database/` son idempotentes y se numeran (`02_...sql`) conforme crezca el esquema.

3. Ejecutar y abrir Swagger en http://localhost:5079/swagger

```bash
dotnet run --project src/RedSocial.API --launch-profile http
```

## Endpoints

- `POST /api/auth/registro` — `{ nombreUsuario, email, password }`
- `POST /api/auth/login` — `{ usuarioOEmail, password }`
- `GET /api/auth/perfil` — requiere `Authorization: Bearer <token>`
