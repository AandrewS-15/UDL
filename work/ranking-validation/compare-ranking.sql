SET NOCOUNT ON;
SELECT ROW_NUMBER() OVER (ORDER BY COALESCE(SUM(n.PuntosAredl),0) DESC, COUNT(r.IdRecord) DESC, j.NombreGD) AS posicion,
       j.IdJugador AS idJugador, j.NombreGD AS nombreGD, j.AvatarUrl AS avatarUrl,
       COALESCE(SUM(n.PuntosAredl),0) AS totalPuntos, COUNT(r.IdRecord) AS cantidadCompletions
FROM dbo.Jugador j
LEFT JOIN dbo.Record r ON r.IdJugador=j.IdJugador AND r.Porcentaje=100
LEFT JOIN dbo.Nivel n ON n.IdNivel=r.IdNivel
GROUP BY j.IdJugador,j.NombreGD,j.AvatarUrl
ORDER BY totalPuntos DESC,cantidadCompletions DESC,j.NombreGD
FOR JSON PATH, INCLUDE_NULL_VALUES;
