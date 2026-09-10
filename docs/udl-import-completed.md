# Importación histórica inicial completada

Se aplicó la autorización del usuario sobre la captura real revisada en el tercer
dry run: `import-data/udl-sheet.third.snapshot.json` (2026-09-10T22:45:48.368Z).
No se mezcló una versión posterior de la planilla con los datos ya revisados.

## Decisiones implementadas

- Alias explícito `Okrun` → `Okarun` en `UdlPlayerAliases.cs`, aplicado a la
  completion de Cataclysm. No es una vinculación por sugerencia tipográfica.
- Players conserva prioridad como fuente canónica. Se incorporan nombres válidos
  exclusivos de Victors con su nombre canónico y AvatarUrl NULL: z, Bruno y
  ThaLion087. Se crea un solo jugador por nombre normalizado. Las filas negativas
  en `PlayerPlan.SourceRows` indican su fila de origen en Victors, no en Players.
- Se mantienen la normalización y reglas de niveles del tercer dry run, incluidos
  los aliases Erebus → Erebus (BoldStep) y Heartbeat → Heartbeat (KrmaL).
- Todos los records históricos nuevos usan Porcentaje=100, VideoUrl=NULL,
  RawFootageUrl=NULL e IdSubmissionOrigen=NULL. FechaAprobacion indica incorporación
  al sistema, no una fecha histórica inventada de la completion.

## ALTER ejecutados

Se verificaron tipos, nulabilidad, índices y FKs antes de ejecutar el script
`sql/prepare-udl-historical-import.sql`. Se aplicaron solamente:

```sql
ALTER TABLE dbo.Jugador ADD AvatarUrl nvarchar(500) NULL;
ALTER TABLE dbo.Record ALTER COLUMN VideoUrl nvarchar(500) NULL;
ALTER TABLE dbo.Record ALTER COLUMN IdSubmissionOrigen int NULL;
```

El script usa transacción, XACT_ABORT y validaciones del esquema, y puede repetirse.
No se recrearon tablas, no se borraron filas, no se quitaron FKs ni se agregaron
migraciones o EsImportado. La clave `UQ_Record_JugadorNivel` y las tres FKs de Record
permanecen activas; las FKs siguen siendo de confianza (`is_not_trusted=0`).
Los hashes de todas las filas fueron idénticos antes y después de los ALTER.

## Importador manual

Disponible solo en Development, con exactamente uno de `--dry-run` o `--apply`.
`--apply` ejecuta `UdlImportService`: transacción Serializable, bloqueo de aplicación
SQL `UDL.HistoricalImport` y nueva lectura/resolución dentro de la transacción.
Un conflicto cancela el lote. Se guarda primero el conjunto de jugadores y luego
los records dentro de la misma transacción. Ante fallo previo al commit se revierte
todo. Los existentes no se reemplazan; solo se completa un AvatarUrl vacío con una
URL válida de Players. La segunda ejecución no cambia fechas ni contenido existente.

```powershell
dotnet run -c Release --no-build -- --import-udl-sheet --apply --sheet-snapshot import-data/udl-sheet.third.snapshot.json --report-prefix import-data/reports/import-real-first --Logging:LogLevel:Microsoft.EntityFrameworkCore=Warning
```

Para planificar sin escribir, sustituir `--apply` por `--dry-run`.
El CLI lee la captura local. La escritura nunca aplica ALTER automáticamente.

## Resultados reales

| Resultado | Primera ejecución | Segunda ejecución |
|---|---:|---:|
| Jugadores nuevos | 66 | 0 |
| Jugadores existentes | 0 | 66 |
| Records nuevos | 781 | 0 |
| Records existentes | 0 | 781 |
| Duplicados de fuente omitidos | 38 | 38 |
| Conflictos | 0 | 0 |

Origen: 63 jugadores de Players + 3 de Victors; 819 celdas de completions,
781 combinaciones únicas al 100%. Las 251 filas de niveles se resolvieron.

## Validaciones finales

- Total SQL: **66 Jugador**, **781 Record**, **0 Submission**.
- Todos los records están al 100%, sin video, raw footage ni submission de origen.
- Cero duplicados Jugador + Nivel y cero usernames duplicados ignorando mayúsculas.
- No existe un jugador Okrun; Okarun tiene Cataclysm al 100%.
- z existe con The Hell Inferno; Bruno existe con Cataclysm.
- ThaLion087 existe una sola vez y tiene exactamente Acu y niwa al 100%.
- Los **63 AvatarUrl de Players coinciden exactamente** con los valores revisados.
  Los únicos **3 AvatarUrl NULL** son z, Bruno y ThaLion087.
- Las filas de Nivel, incluidos PuntosAredl, y de HistorialNivel permanecen
  idénticas por hash al estado anterior; Submission permanece vacío.
- La segunda ejecución dejó todas las filas y columnas idénticas a la primera.
  SHA-256 de ambos archivos de verificación:
  `CA46116E091B453D813EE674016AF353AA000298129ECFC1961E9DC38A4AED9D`.
- Compilación Release: 0 errores, 0 advertencias; 63 checks del importador aprobados
  y todos los checks AREDL aprobados. Las dos ejecuciones reales verifican además
  la integración EF/SQL y la idempotencia transaccional.

## Warnings restantes

Solo datos repetidos de la fuente: una fila repetida de Cataclysm y seis grupos
de victors repetidos: Fif en Astral Divinity, Inc0gnit en Aftermath, EmilioRory,
WinRAR y Neuronfish en Cataclysm, Santiricca en Rauchkammer. Se omitieron sin crear
duplicados. Hay cero discrepancias de victor count y cero conflictos pendientes.

## Evidencia y archivos

- `import-data/reports/import-real-first.txt` y `.json`: primera importación.
- `import-data/reports/import-real-second.txt` y `.json`: ejecución idempotente.
- `final-sql-validation.txt`: conteos, duplicados, casos especiales y constraints.
- `avatar-validation.txt` y `avatars-sql.json`: comprobación exacta de avatares.
- `sql-before-schema.txt` / `sql-after-schema.txt`: sin pérdida de datos por ALTER.
- `sql-after-first-import.txt` / `sql-after-second-import.txt`: prueba del no-op.
- `sql/verify-historical-import-result.sql`: verificaciones SQL reproducibles.

Código agregado: `UdlPlayerAliases.cs` y `UdlImportService.cs`. Ajustados planner,
lector transaccional, informes, Program y tests. Los modelos ya estaban preparados.
No se modificaron frontend, Identity, ranking, moderación ni submissions funcionales.
