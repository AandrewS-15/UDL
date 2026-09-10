-- Verificación de solo lectura después de la importación inicial.
SET NOCOUNT ON;
SELECT (SELECT COUNT(*) FROM dbo.Jugador) AS Jugadores,
       (SELECT COUNT(*) FROM dbo.Record) AS Records,
       (SELECT COUNT(*) FROM dbo.Submission) AS Submissions,
       (SELECT COUNT(*) FROM dbo.Jugador WHERE AvatarUrl IS NULL) AS AvataresNull;
SELECT COUNT(*) AS RecordsNoHistoricos FROM dbo.Record
WHERE Porcentaje <> 100 OR VideoUrl IS NOT NULL OR RawFootageUrl IS NOT NULL OR IdSubmissionOrigen IS NOT NULL;
SELECT COUNT(*) AS CombinacionesDuplicadas FROM
    (SELECT IdJugador,IdNivel FROM dbo.Record GROUP BY IdJugador,IdNivel HAVING COUNT(*)>1) d;
SELECT COUNT(*) AS UsernamesDuplicados FROM
    (SELECT UPPER(LTRIM(RTRIM(NombreGD))) AS Nombre FROM dbo.Jugador
     GROUP BY UPPER(LTRIM(RTRIM(NombreGD))) HAVING COUNT(*)>1) d;
SELECT COUNT(*) AS JugadorOkrun FROM dbo.Jugador WHERE NombreGD=N'Okrun';
SELECT j.NombreGD,n.Nombre,r.Porcentaje,r.VideoUrl,r.IdSubmissionOrigen
FROM dbo.Record r JOIN dbo.Jugador j ON j.IdJugador=r.IdJugador
JOIN dbo.Nivel n ON n.IdNivel=r.IdNivel
WHERE j.NombreGD IN (N'z',N'Bruno',N'ThaLion087')
   OR (j.NombreGD=N'Okarun' AND n.Nombre=N'Cataclysm')
ORDER BY j.NombreGD,n.Nombre;
SELECT t.name AS Tabla,c.name AS Columna,TYPE_NAME(c.user_type_id) AS Tipo,c.max_length,c.is_nullable
FROM sys.columns c JOIN sys.tables t ON t.object_id=c.object_id
WHERE (t.name=N'Jugador' AND c.name=N'AvatarUrl')
   OR (t.name=N'Record' AND c.name IN (N'VideoUrl',N'IdSubmissionOrigen'));
SELECT name,is_disabled,is_not_trusted FROM sys.foreign_keys WHERE parent_object_id=OBJECT_ID(N'dbo.Record');
SELECT name,is_unique,is_disabled FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Record') AND is_unique=1;
