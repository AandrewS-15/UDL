-- Aplicado manualmente con autorización del usuario al cerrar la importación inicial.
-- Script repetible; no lo ejecuta automáticamente el importador ni el dry run.
-- No importa datos, no recrea tablas, no cambia índices, FKs ni valores existentes.
USE [UDL];
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.Jugador', N'U') IS NULL OR OBJECT_ID(N'dbo.Record', N'U') IS NULL
    THROW 51000, 'Faltan tablas esperadas de UDL.', 1;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Record')
    AND name = N'VideoUrl' AND TYPE_NAME(user_type_id) = N'nvarchar' AND max_length = 1000)
    THROW 51000, 'Record.VideoUrl no es nvarchar(500); revisar esquema antes de continuar.', 1;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Record')
    AND name = N'IdSubmissionOrigen' AND TYPE_NAME(user_type_id) = N'int')
    THROW 51000, 'Record.IdSubmissionOrigen no es int; revisar esquema.', 1;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Jugador') AND name = N'AvatarUrl')
    ALTER TABLE dbo.Jugador ADD AvatarUrl nvarchar(500) NULL;
ELSE IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Jugador')
    AND name = N'AvatarUrl' AND TYPE_NAME(user_type_id) = N'nvarchar' AND max_length = 1000 AND is_nullable = 1)
    THROW 51000, 'Jugador.AvatarUrl ya existe con otro tipo o nulabilidad; revisar esquema.', 1;

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Record') AND name = N'VideoUrl' AND is_nullable = 0)
    ALTER TABLE dbo.Record ALTER COLUMN VideoUrl nvarchar(500) NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Record') AND name = N'IdSubmissionOrigen' AND is_nullable = 0)
    ALTER TABLE dbo.Record ALTER COLUMN IdSubmissionOrigen int NULL;

COMMIT TRANSACTION;
-- UQ_Record_JugadorNivel y FK_Record_Submission permanecen intactas.
-- IdSubmissionOrigen NULL representa un record histórico sin submission de origen.
