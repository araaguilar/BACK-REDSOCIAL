param(
    [string]$ConnectionString = "Server=localhost,PUERTO;Database=RedSocialDB;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True",
    [switch]$Force
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$backendDir = Split-Path -Parent $scriptDir
$apiDir = Join-Path $backendDir "src\RedSocial.API"
$localConfigPath = Join-Path $apiDir "appsettings.Local.json"

if ((Test-Path -LiteralPath $localConfigPath) -and -not $Force) {
    Write-Host "Ya existe appsettings.Local.json. No se modifico."
    Write-Host "Usa -Force si quieres regenerar la clave JWT y sobrescribirlo."
    exit 0
}

$randomBytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Fill($randomBytes)
$jwtKey = [Convert]::ToBase64String($randomBytes)

$config = [ordered]@{
    ConnectionStrings = [ordered]@{
        DefaultConnection = $ConnectionString
    }
    Jwt = [ordered]@{
        Key = $jwtKey
    }
}

$json = $config | ConvertTo-Json -Depth 5
Set-Content -LiteralPath $localConfigPath -Value $json -Encoding UTF8

Write-Host "Listo: se creo appsettings.Local.json con una Jwt:Key segura."
Write-Host "Revisa DefaultConnection si tu SQL Server usa otro host, puerto, usuario o password."
