-- RedSocial - Script inicial
-- Ejecutar en SQL Server Management Studio.

IF DB_ID('RedSocialDB') IS NULL
    CREATE DATABASE RedSocialDB;
GO

USE RedSocialDB;
GO

IF OBJECT_ID('dbo.Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        IdUsuario        INT IDENTITY(1,1) CONSTRAINT PK_Usuarios PRIMARY KEY,
        NombreUsuario    NVARCHAR(30)  NOT NULL,
        NombrePerfil     NVARCHAR(60)  NOT NULL,
        Email            NVARCHAR(100) NOT NULL,
        -- Hash BCrypt (60 caracteres ASCII). NUNCA la contraseña en texto plano.
        PasswordHash     VARCHAR(60)   NOT NULL,
        FechaNacimiento  DATE          NULL,
        Rol              NVARCHAR(20)  NOT NULL CONSTRAINT DF_Usuarios_Rol DEFAULT 'usuario',
        EmailVerificado  BIT           NOT NULL CONSTRAINT DF_Usuarios_EmailVerificado DEFAULT 0,
        Activo           BIT           NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT 1,
        FechaRegistro    DATETIME2     NOT NULL CONSTRAINT DF_Usuarios_FechaRegistro DEFAULT SYSUTCDATETIME(),
        UltimoLogin      DATETIME2     NULL,
        CONSTRAINT UQ_Usuarios_NombreUsuario UNIQUE (NombreUsuario),
        CONSTRAINT UQ_Usuarios_Email UNIQUE (Email)
    );
END
GO

IF OBJECT_ID('dbo.VerificacionesEmail', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.VerificacionesEmail (
        IdVerificacionEmail INT IDENTITY(1,1) CONSTRAINT PK_VerificacionesEmail PRIMARY KEY,
        Email               NVARCHAR(100) NOT NULL,
        CodigoHash          VARCHAR(64)   NOT NULL,
        TokenVerificacion   VARCHAR(64)   NOT NULL,
        ExpiraEn            DATETIME2     NOT NULL,
        IntentosFallidos    INT           NOT NULL CONSTRAINT DF_VerificacionesEmail_IntentosFallidos DEFAULT 0,
        FechaCreacion       DATETIME2     NOT NULL CONSTRAINT DF_VerificacionesEmail_FechaCreacion DEFAULT SYSUTCDATETIME(),
        FechaVerificacion   DATETIME2     NULL,
        Usado               BIT           NOT NULL CONSTRAINT DF_VerificacionesEmail_Usado DEFAULT 0,
        CONSTRAINT UQ_VerificacionesEmail_TokenVerificacion UNIQUE (TokenVerificacion)
    );

    CREATE INDEX IX_VerificacionesEmail_Email ON dbo.VerificacionesEmail (Email);
END
GO
