# Segundo dry run de Uruguay Demon List

Histórico: la estrategia y el dato de avatares de este informe fueron sustituidos
por el [tercer dry run](udl-third-dry-run.md). Las celdas de avatar contenían
fórmulas IMAGE, no estaban realmente vacías.

Solo lectura: no se ejecutó importación real ni ALTER TABLE. Esta versión sustituye
las reglas de matching del primer dry run. Se conserva su captura e informe original.

## Cambios de resolución

- Jugadores: trim, Unicode NFC, eliminación de caracteres Unicode de categoría
  Format (incluidos los de fuera del BMP), comparación sin mayúsculas/minúsculas.
  Solo para Victors se ignora un único punto final; no se quitan puntos internos.
  `CanonicalUsername` conserva la capitalización y los caracteres del nombre de
  Players tras limpiar espacios extremos y caracteres de formato. El nombre de
  Victors se mantiene por separado para trazabilidad.
- Niveles: limpiar formato y espacios innecesarios, conservando mayúsculas,
  signos, tildes y calificadores. La clave es case-sensitive.
- Una coincidencia exacta tiene prioridad. Solo cuando no existe se consulta
  `Services/Import/UdlLevelAliases.cs`, con dos aliases explícitos:
  `Heartbeat` → `Heartbeat (KrmaL)` y `Erebus` → `Erebus (BoldStep)`.
  El segundo representa la identidad que confirmó el usuario y el nombre real
  almacenado en SQL. `ErebuS (Platnuu)` no es equivalente.
- Una coincidencia ambigua no se sustituye por un alias. Los candidatos sugeridos,
  posición histórica, GeometryDashId y Publisher son informativos; no deciden
  identidad. No hay fuzzy matching ni generación automática de aliases.
- La deduplicación por Jugador + Nivel ocurre después de resolver los nombres.
  Los duplicados de filas sin nivel resuelto siguen apareciendo como warnings,
  aunque sus records se clasifican como omitidos por nivel sin resolver.

## Fuente y comando

Se consultaron las dimensiones actuales y se volvieron a leer completamente
Players!A1:BH994 y Victors!A1:BO312 mediante Google Sheets. El segundo bloque de
Players (501–994) estaba vacío. Captura UTC: 2026-09-10T22:36:18.815Z.
La captura es `import-data/udl-sheet.second.snapshot.json`; el comando no consulta
Google Sheets automáticamente, sino que lee esa captura real reproducible.

```powershell
dotnet run -c Release --no-build -- --import-udl-sheet --dry-run --sheet-snapshot import-data/udl-sheet.second.snapshot.json --report-prefix import-data/reports/udl-second-dry-run
```

## Resumen completo

| Jugadores | Cantidad |
|---|---:|
| Detectados / únicos | 63 / 63 |
| Nuevos / existentes | 63 / 0 |
| Conflictos en Players | 0 |
| Conflictos de jugador en Victors | 5 |
| Con AvatarUrl / sin AvatarUrl | 0 / 63 |
| Avatares que podrían actualizarse | 0 |

| Niveles | Cantidad |
|---|---:|
| Filas de Victors | 251 |
| Resueltos correctamente | 234 |
| No encontrados | 17 |
| Ambiguos / otros conflictos | 0 / 0 |

| Records | Cantidad |
|---|---:|
| Completions detectadas | 819 |
| Records nuevos | 692 |
| Records existentes | 0 |
| Duplicados omitidos (combinaciones resueltas) | 36 |
| Records omitidos, total | 127 |

Los tres casos originales quedaron resueltos: Erebus → IdNivel 429 por alias,
Heartbeat → IdNivel 1073 por alias y `2 1 1` → IdNivel 542 por coincidencia exacta
después de eliminar U+200E. La cantidad de records nuevos baja respecto del primer
dry run porque ahora los nombres de niveles distinguen mayúsculas y minúsculas.
Los duplicados aumentan porque los usernames con punto final ahora se resuelven.

## Conflictos de nivel restantes

Motivo común: sin coincidencia exacta case-sensitive y sin alias explícito.
Todos los siguientes son candidatos **para revisión**, no vinculaciones:

| Fila | Nombre planilla | AREDL histórico | Candidatos SQL (IdNivel: nombre) |
|---|---|---:|---|
| 22 | Kowareta | 300 | 303: kowareta |
| 93 | Moment | 823 | 826: moment |
| 94 | Azurite (Royen) | 824 | 827: Azurite (royen); 1510: Azurite (Sillow) |
| 96 | Gravity | 831 | 834: gravity |
| 99 | Quaoar (viprin) | 843 | 568: Quaoar (Flosia); 846: Quaoar (ViPriN) |
| 118 | Lunar | 968 | 971: lunar |
| 121 | Aftermath | 988 | 991: aftermath |
| 126 | Panasonic | 1008 | 1011: PanaSonic |
| 137 | Under Lavaland | 1093 | 1096: Under lavaland |
| 146 | Rivers of Nazareth | 1157 | 1160: RIVERS OF NAZARETH |
| 210 | Make it Drop | 1428 | 1430: Make It Drop |
| 215 | reverie | 1443 | 1445: Reverie |
| 226 | Glisten | 1493 | 1494: GLISTEN |
| 229 | 9Blue | 1504 | 1505: 9blue |
| 241 | Niwa | 1533 | 1535: niwa |
| 246 | Troll Level | 1554 | 1557: troll level |
| 254 | Rauchkammer | 1572 | 1575: rauchkammer |

El TXT/JSON completo incluye AredlId, GeometryDashId y posición actual de cada
candidato. Publisher está vacío en SQL y se informa como desconocido.

## Conflictos de jugador restantes

Todos están ausentes de Players y no tienen candidatos en SQL:

| Fila | Nombre planilla | Nivel | AREDL histórico |
|---|---|---|---:|
| 64 | z | The Hell Inferno | 653 |
| 221 | Bruno | Cataclysm | 1477 |
| 223 | Okrun | Cataclysm | 1477 |
| 241 | ThaLion087 | Niwa | 1533 |
| 250 | ThaLion087 | Acu | 1567 |

La regla para `Thalion087.` está implementada y probada. En la captura real,
`ThaLion087` aparece dos veces en Victors pero no tiene fila canónica en Players.
No se inventó ese jugador ni se sustituyó por otro nombre.

## Warnings

- Victor count: **0** discrepancias. Se cuentan celdas no vacías antes de deduplicar.
- Filas duplicadas: **1**, Cataclysm, fila 223.
- Victors repetidos: **6** grupos: Fif en Astral Divinity; Inc0gnit en Aftermath;
  EmilioRory en Cataclysm fila 221; WinRAR y Neuronfish en Cataclysm fila 223;
  Santiricca en Rauchkammer. EmilioRory se reconoce como repetido tras quitar el punto.
- Otros: **1**, la columna AvatarUrl aún no existe en SQL. El dry run la trata
  como NULL y no intenta crearla. No se inventan URLs ni avatares.

## Verificación y SQL pendiente

Compilación Release: 0 errores y 0 advertencias. Pasaron **53 checks de importación**
y todos los checks de AREDL. Se cubren normalización, aliases, prioridad exacta,
ambigüedad, Erebus/ErebuS, caracteres de formato, username canónico, puntos internos,
deduplicación e idempotencia conceptual; también avatares y ausencia de fuzzy/rank matching.

Los archivos `import-data/reports/sql-second-before.txt` y `sql-second-after.txt`
son idénticos. SHA-256 de ambos:
`C36D7779F1A7CE89587E596EF1489EF7F715878EFECD643E21F2465B33853218`.
Contienen hashes de filas y columnas: SQL conserva 0 jugadores, 1606 niveles,
0 submissions, 0 records y 1606 historiales. No hubo modificaciones SQL.

Sigue pendiente, sin cambios ni ejecución: `sql/prepare-udl-historical-import.sql`.
Agrega AvatarUrl NVARCHAR(500) NULL y permite NULL en Record.VideoUrl e
IdSubmissionOrigen. Mantiene UNIQUE(IdJugador, IdNivel) y la FK de submission.
No se agrega EsImportado ni se crean evidencias ficticias.

## Archivos de esta segunda revisión

Modificados: `Services/Import/ImportNames.cs`, `ImportPlan.cs`,
`UdlImportPlanner.cs`, `UdlImportDatabaseReader.cs`, `UdlImportReport.cs`,
`tests/UDL.Import.Checks/Program.cs`, `docs/udl-historical-import.md`, `README.md`.

Creados: `Services/Import/UdlLevelAliases.cs`, esta documentación,
`import-data/udl-sheet.second.snapshot.json`, informes `udl-second-dry-run.txt`
y `.json`, `second-console.txt`, `sql-second-before.txt` y `sql-second-after.txt`.

No se modificaron modelos, esquema, puntos UDL, frontend, Identity ni lógica de
submissions/moderación en esta segunda revisión.
