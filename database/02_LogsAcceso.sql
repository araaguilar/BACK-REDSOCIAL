-- Ejecutar después de 01_CrearBaseDatos.sql. No elimina ni modifica registros existentes.
USE RedSocialDB;
GO
IF OBJECT_ID('dbo.LogsAcceso', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.LogsAcceso (
        IdLogAcceso BIGINT IDENTITY(1,1) CONSTRAINT PK_LogsAcceso PRIMARY KEY,
        IdUsuario INT NULL,
        Identificador NVARCHAR(100) NOT NULL,
        Evento NVARCHAR(30) NOT NULL,
        Exitoso BIT NOT NULL,
        FechaUtc DATETIMEOFFSET NOT NULL,
        DireccionIp VARCHAR(45) NULL
    );
    CREATE INDEX IX_LogsAcceso_FechaUtc_IdLogAcceso ON dbo.LogsAcceso(FechaUtc, IdLogAcceso);
    CREATE INDEX IX_LogsAcceso_IdUsuario ON dbo.LogsAcceso(IdUsuario);
END;
GO
