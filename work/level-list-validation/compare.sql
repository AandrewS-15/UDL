SET NOCOUNT ON;
SELECT ROW_NUMBER() OVER(ORDER BY n.PosicionAredl,n.Nombre) posicionUy,
n.IdNivel idNivel,n.Nombre nombre,n.PosicionAredl posicionAredl,n.PuntosAredl puntosAredl,
COUNT(r.IdRecord) victorCount,
CASE WHEN n.PosicionAredl BETWEEN 1 AND 75 THEN 'Main List' WHEN n.PosicionAredl BETWEEN 76 AND 150 THEN 'Extended' WHEN n.PosicionAredl >= 151 THEN 'Legacy' ELSE 'Sin posición' END categoria
FROM dbo.Nivel n JOIN dbo.Record r ON r.IdNivel=n.IdNivel AND r.Porcentaje=100
GROUP BY n.IdNivel,n.Nombre,n.PosicionAredl,n.PuntosAredl
ORDER BY n.PosicionAredl,n.Nombre FOR JSON PATH,INCLUDE_NULL_VALUES;
