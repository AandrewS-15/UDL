-- Evidencia de solo lectura: ejecutar antes y después del dry run y comparar salidas.
SET NOCOUNT ON;
SELECT 'Jugador' AS Tabla, COUNT_BIG(*) AS Filas,
    CONVERT(varchar(64), HASHBYTES('SHA2_256', (SELECT * FROM dbo.Jugador ORDER BY IdJugador FOR JSON PATH, INCLUDE_NULL_VALUES)), 2) AS HashFilas FROM dbo.Jugador;
SELECT 'Nivel' AS Tabla, COUNT_BIG(*) AS Filas,
    CONVERT(varchar(64), HASHBYTES('SHA2_256', (SELECT * FROM dbo.Nivel ORDER BY IdNivel FOR JSON PATH, INCLUDE_NULL_VALUES)), 2) AS HashFilas FROM dbo.Nivel;
SELECT 'Submission' AS Tabla, COUNT_BIG(*) AS Filas,
    CONVERT(varchar(64), HASHBYTES('SHA2_256', (SELECT * FROM dbo.Submission ORDER BY IdSubmission FOR JSON PATH, INCLUDE_NULL_VALUES)), 2) AS HashFilas FROM dbo.Submission;
SELECT 'Record' AS Tabla, COUNT_BIG(*) AS Filas,
    CONVERT(varchar(64), HASHBYTES('SHA2_256', (SELECT * FROM dbo.Record ORDER BY IdRecord FOR JSON PATH, INCLUDE_NULL_VALUES)), 2) AS HashFilas FROM dbo.Record;
SELECT 'HistorialNivel' AS Tabla, COUNT_BIG(*) AS Filas,
    CONVERT(varchar(64), HASHBYTES('SHA2_256', (SELECT * FROM dbo.HistorialNivel ORDER BY IdHistorial FOR JSON PATH, INCLUDE_NULL_VALUES)), 2) AS HashFilas FROM dbo.HistorialNivel;
SELECT 'Schema' AS Tabla, CONVERT(varchar(64), HASHBYTES('SHA2_256',
    (SELECT t.name AS Tabla, c.* FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id
     WHERE t.name IN ('Jugador', 'Nivel', 'Submission', 'Record', 'HistorialNivel')
     ORDER BY t.name, c.column_id FOR JSON PATH, INCLUDE_NULL_VALUES)), 2) AS HashColumnas;
