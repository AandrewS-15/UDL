# Ranking real de jugadores

La tabla existente de **Jugadores** (`/#players`) consulta `GET /api/players/ranking`.
La sección Clasificación conserva su estructura informativa y enlaza a esa tabla;
su explicación de desempates ahora refleja las reglas reales.

## Consulta

`PlayerRankingQuery` realiza un único SELECT mediante EF, sin tracking, sin cargar
colecciones y sin consultas N+1. Para cada Jugador calcula en SQL:

- SUM de Nivel.PuntosAredl en sus Record con Porcentaje=100 (COALESCE a cero).
- COUNT de esos Record.
- Orden TotalPuntos DESC, CantidadCompletions DESC, NombreGD ASC.

Record representa un record aprobado, incluidos los históricos sin Submission.
No se filtra por Submission ni se excluyen jugadores sin records. Joako aparece
con 0 puntos y 0 completions. La posición se asigna secuencialmente después de
materializar el resultado ordenado. No hay columnas nuevas ni SaveChanges.

La tabla obtiene un ViewModel de lectura y muestra los números con hasta un
decimal, sin ceros finales. Los nombres quedan sin link para evitar dirigirlos
a los perfiles simulados. El ranking simulado utilizado por otras secciones se
conserva aislado: no alimenta la tabla real.

## Diseño y avatares

Se mantienen navbar, colores, tipografía, fondo, tabla, clases y búsqueda existentes.
Solo se agrega la imagen junto al nombre y los estados de carga/error/reintento.
Los avatares usan object-fit:cover y un contenedor circular de tamaño fijo. Si la
URL es NULL, inválida o falla, queda visible la inicial. No se modifica SQL ni se
guarda el fallback. Se verificó la carga de imágenes reales y el fallback de Gekura
(su imagen remota falló en esta sesión), además de los jugadores con AvatarUrl NULL.

## Validación

- Compilación Release correcta, cero errores y advertencias.
- Checks del ranking: decimales, exclusión de progresos, jugadores sin records,
  ambos desempates, AvatarUrl y traducción a SQL. Pasaron también los 63 checks del
  importador y los checks AREDL existentes. JavaScript pasó node --check.
- Servidor local verificado en http://localhost:5083/#players.
- 66 jugadores visibles. Las 66 filas del endpoint se compararon campo por campo
  con una consulta SQL independiente de LEFT JOIN + GROUP BY + ROW_NUMBER:
  posición, IdJugador, nombre, avatar, puntos y completions coinciden.
- Se verificó visualmente el top 10, avatares, búsqueda, estado sin resultados,
  limpieza de búsqueda y fallback para ThaLion087. En viewport de 390px no hubo
  desbordamiento horizontal. Se restauró el viewport habitual al terminar.
- Logs: una consulta EF por petición al endpoint; no hay consultas por fila.
- `work/ranking-validation/sql-before.txt` y `sql-after.txt` son idénticos:
  incluyen conteos y hashes de filas y columnas de las cinco tablas. No se
  modificó SQL Server ni se persistieron totales/posiciones calculadas.
- No se cambiaron AredlSyncService, UdlPointsCalculator, schema ni entidades EF.

## Top 10 verificado

| UY | Jugador | Puntos | Completions |
|---|---|---:|---:|
| 1 | Santiricca | 7918.8 | 66 |
| 2 | cxve | 5483 | 65 |
| 3 | Fif | 4820 | 44 |
| 4 | Ardizz | 4519.3 | 1 |
| 5 | Gekura | 4300.7 | 26 |
| 6 | Neuronfish | 3428.4 | 30 |
| 7 | LunatikSick | 3333.9 | 40 |
| 8 | Salva | 2974.1 | 8 |
| 9 | Pissa | 2900.8 | 27 |
| 10 | Andres Beltran | 2525.9 | 17 |

## Archivos de esta etapa

Agregados: `Controllers/PlayerRankingController.cs`,
`Services/Ranking/PlayerRankingQuery.cs`, `ViewModels/PlayerRankingViewModel.cs`,
`wwwroot/udl-ranking.js`, `tests/UDL.Ranking.Checks/Program.cs` y `.csproj`,
este documento y las evidencias en `work/ranking-validation/`.

Modificados: `Program.cs` (registro del servicio), `Views/Home/Index.cshtml`
(carga del script), `wwwroot/udl-app.js` (retirar solo la tabla simulada),
`wwwroot/udl-account.js` (texto de desempates), `wwwroot/udl-styles.css`
(avatar y alineación), `README.md`.
