# Logs de acceso — instalación y uso

Se agregaron registros de inicio de sesión y reactivación de cuenta con usuario, fecha, hora, dirección IP y resultado. Se consultan en **Panel administrador → Logs de acceso**. Incluye filtros por identificador/IP, resultado y rango de fechas, paginación de 20 registros y botón Actualizar.

## Instalación

1. Haz una copia de tu proyecto y de tu base de datos.
2. Descomprime FRONT-REDSOCIAL-con-logs.zip y BACK-REDSOCIAL-con-logs.zip. Contienen el código de tus proyectos con los cambios; no incluyen node_modules, .vs, bin, obj ni credenciales locales. Conserva tus archivos .env y appsettings.Local.json.
3. En SQL Server Management Studio ejecuta **database/02_LogsAcceso.sql**, incluido en el backend. Si usas una base con otro nombre, ajusta `USE RedSocialDB` antes de ejecutar. Si partes de una base nueva, ejecuta primero 01_CrearBaseDatos.sql. El script 02 puede repetirse y conserva los registros.
4. Desde la carpeta del backend:

```powershell
dotnet restore RedSocial.sln
dotnet run --project src/RedSocial.API --launch-profile http
```

5. Desde la carpeta del frontend:

```powershell
npm ci
npm run dev
```

6. Inicia sesión, entra como administrador y selecciona **Logs de acceso**. Haz también un intento con contraseña incorrecta y actualiza la lista para comprobar ambos resultados.

## Datos registrados

- Usuario confirmado e ID en accesos exitosos.
- Identificador escrito en el formulario en intentos fallidos. Ese dato no confirma la identidad de quien hizo el intento; se muestra así en el panel.
- Evento: inicio de sesión o reactivación.
- Resultado: exitoso o fallido. Una cuenta desactivada se registra como acceso fallido.
- Fecha UTC generada por el servidor, guardada con zona horaria. El panel la convierte a la zona local del navegador y muestra fecha, hora y segundos.
- IP IPv4 o IPv6 obtenida por el backend; si no está disponible se indica en el panel.

No se guardan contraseñas, JWT ni cuerpos de petición. La consulta requiere el rol admin y no permite caché HTTP. Se conservan los identificadores del historial aunque cambie o se elimine una cuenta.

## Dirección IP detrás de un proxy

En acceso directo no hace falta configuración. En localhost es normal ver `::1` o `127.0.0.1`. Si publicas detrás de un proxy, agrega su IP real en appsettings.Local.json (desarrollo) o en la configuración del servidor (producción):

```json
{
  "ReverseProxy": {
    "KnownProxies": ["10.0.0.5"]
  }
}
```

Sustituye 10.0.0.5 por la dirección del proxy que se conecta al backend. El proxy debe sobrescribir o sanear X-Forwarded-For y enviar X-Forwarded-Proto. La configuración incluida contempla un salto de proxy. Sin un proxy configurado, se usa la dirección de la conexión y no se lee directamente una cabecera enviada por el cliente. Para varios saltos se debe ajustar la lista de confianza y ForwardLimit según tu infraestructura.

Referencia oficial: https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer

## Alcance

Los registros comienzan después de instalar el cambio; no es posible reconstruir IP históricas que no se guardaron. Se registran respuestas normales del login y la reactivación, exitosas o fallidas, con formularios válidos. No se registran navegación anónima, cada visita a una página, cierre de sesión, formularios rechazados por validación HTTP 400 ni errores internos previos al resultado del servicio. Los fallos muestran un identificador intentado, no un usuario autenticado.

Ejecuta el script SQL antes de iniciar: guardar el log es obligatorio para devolver la respuesta de login. Si falla su escritura, la API devuelve su error genérico y no entrega el token; algunos cambios previos del servicio, como UltimoLogin o la reactivación, podrían haberse guardado ya. No se incluye purga automática.

## Consulta de API

`GET /api/admin/logs-acceso?pagina=1&q=ana&exitoso=true&desde=2026-10-20T00:00:00Z&hasta=2026-10-21T00:00:00Z`

Requiere `Authorization: Bearer <token-admin>`. Los parámetros de filtro son opcionales. La respuesta contiene `exito`, y `datos` con `items`, `pagina`, `total`, `cantidad` y `totalPaginas`.

## Verificación

- Frontend: compilación de producción con `npm run build -- --configLoader native` y revisión de los archivos nuevos con oxlint.
- Backend: compilación y comprobaciones ejecutables de identidad, accesos fallidos, fechas UTC, IP IPv4/IPv6, proxies confiables/no confiables, autorización del controlador y mapeo EF.

Para repetir las comprobaciones del backend:

```powershell
dotnet run --project tests/AccessLogs.Checks
```

La verificación automatizada no se conecta a SQL Server. Falta ejecutar el script y comprobar un login con tu base de datos y la IP en tu entorno publicado.
