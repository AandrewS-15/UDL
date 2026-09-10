# Importación histórica UDL: fase de revisión

Este documento conserva las reglas y resultados del **primer** dry run.
Las reglas vigentes y el resultado actualizado están en
[Segundo dry run](udl-second-dry-run.md).
La versión vigente es el [tercer dry run](udl-third-dry-run.md), que además
corrige la lectura de los avatares IMAGE previamente informados como vacíos.

Esta fase solo construye un plan. No implementa ni ejecuta inserciones, updates,
submissions, evidencia ficticia, ni cambios de schema. Todas las completions se
planifican al 100%. `PuntosAredl`, rankings y frontend permanecen intactos.

## Fuente y reproducción

Fuente real: https://docs.google.com/spreadsheets/d/1sLqNc3NxZUY1K00KOGJhRIq1FKkDEwH4npjuAVDCQvI/edit

La captura `import-data/udl-sheet.snapshot.json` se obtuvo mediante el conector de
Google Drive/Sheets, leyendo todos los valores de `Players!A1:BH994` (en dos
bloques: filas 1–500 y 501–994) y `Victors!A1:BO312`. Google omite las filas vacías
al final de cada respuesta; el segundo bloque de Players estaba completamente
vacío. La captura incluye la URL, rangos y fecha UTC. No se leyeron como fuente
Leaderboard, database, Challenge List ni EXTREME DEMON ALPHABET 2026.

El comando lee esta captura local, **no refresca automáticamente Google Sheets**.
Para repetir con datos nuevos, volver a capturar los rangos completos de ambas
hojas mediante Sheets después de consultar sus dimensiones y actualizar el JSON.
No se deben combinar lecturas parciales ni capturas de distintas versiones.
El formato conserva matrices de celdas y encabezados originales, incluidos
vacíos intermedios. El parser consume solo username/avatar, nombre del nivel,
rank histórico, victor count y las columnas F en adelante. Ignora puntos y rangos
derivados, aunque sus valores originales estén en la captura para trazabilidad.

Desde la raíz del proyecto:

```powershell
dotnet build -c Release
dotnet run --project tests/UDL.Import.Checks -c Release
dotnet run --project tests/UDL.Aredl.Checks -c Release
dotnet run -c Release --no-build -- --import-udl-sheet --dry-run --sheet-snapshot import-data/udl-sheet.snapshot.json --report-prefix import-data/reports/udl-dry-run
```

Solo Development; el perfil local lo configura. Sin `--dry-run`, en Production,
o combinando este modo con sync/verificación, se rechaza la ejecución.
Los conflictos son resultados válidos de un dry run: se listan y no provocan
inserciones. Los errores de entrada/conexión dan código de salida 1.

## Resolución y avatares

- Usernames: trim, Unicode NFC y comparación sin mayúsculas; no se quitan puntos,
  tildes ni espacios internos. Variantes de mayúsculas se agrupan. Duplicados SQL
  o avatares contradictorios en Players generan conflicto.
- Niveles: NFC, case-insensitive y colapso de whitespace. Se mantienen signos,
  calificadores `(Solo)`/`(2P)` y caracteres especiales. Caracteres invisibles
  requieren revisión. Solo una coincidencia normalizada única y modalidad
  compatible permite vincular. El rank histórico nunca decide la identidad.
- Las sugerencias que omiten calificadores, marcas de formato o puntos finales
  son exclusivamente informativas: jamás se usan para vincular.
- Un victor ausente de Players solo se resuelve si existe un único jugador SQL
  con ese username. En cualquier otro caso se omite y se registra el conflicto;
  no se crean jugadores desde posibles errores tipográficos de Victors.
- Se deduplica por jugador resuelto y IdNivel a través de todo el lote. Si ya
  existe la combinación en SQL, no se crea ni reemplaza un record, incluso si
  SQL tiene un porcentaje menor que 100 (se avisa en ese caso).
- `victor count` se compara con todas las celdas no vacías, antes de deduplicar.
  Discrepancias, nombres repetidos y filas repetidas generan warnings.
- Avatares: URL HTTP(S) absoluta, sin credenciales, máximo 500 caracteres. Vacío
  equivale a NULL; inválido se omite con warning. Un avatar nuevo para un jugador
  existente sin avatar se marca como posible actualización. Si SQL ya tiene uno,
  se conserva; una diferencia se advierte. No se descargan imágenes.

## Cambios SQL preparados, NO ejecutados

Revisar `sql/prepare-udl-historical-import.sql` antes de cualquier importación real:

1. `ALTER TABLE dbo.Jugador ADD AvatarUrl NVARCHAR(500) NULL` si no existe.
2. `ALTER TABLE dbo.Record ALTER COLUMN VideoUrl NVARCHAR(500) NULL` si era obligatorio.
3. `ALTER TABLE dbo.Record ALTER COLUMN IdSubmissionOrigen INT NULL` si era obligatorio.

El script valida tipos previos, es repetible, usa transacción y `XACT_ABORT`, y no
elimina tablas, filas, constraints ni índices. Conserva `UQ_Record_JugadorNivel`
y `FK_Record_Submission`. No se crea una migración EF. Los modelos EF reflejan
estos cambios futuros. El lector del dry run consulta si AvatarUrl existe antes
de seleccionarlo, por lo que funciona con ambos esquemas. Las consultas EF que
materialicen Jugador completo requerirán aplicar el ALTER antes de usar AvatarUrl.

No se agrega `EsImportado`: en esta etapa `IdSubmissionOrigen = NULL` representa
un histórico sin submission de origen. No se infieren fechas de completion;
`FechaAprobacion` no debe presentarse como fecha histórica de la hazaña. La fase
de escritura queda pendiente de revisión y autorización de los conflictos y del
script. No hay un flag para ejecutar la importación real en esta versión.

## Resultado de la captura del 10 de septiembre de 2026

| Concepto | Resultado |
|---|---:|
| Players detectados / únicos | 63 / 63 |
| Jugadores nuevos / existentes / conflictos en Players | 63 / 0 / 0 |
| Conflictos de jugador en Victors (ocurrencias) | 37 |
| Avatares no vacíos / actualizaciones propuestas | 0 / 0 |
| Filas de niveles | 251 |
| Resueltas / no encontradas / ambiguas / otros conflictos | 248 / 2 / 0 / 1 |
| Completions detectadas | 819 |
| Records nuevos / existentes / omitidos | 764 / 0 / 55 |
| Omitidos por jugador / nivel / duplicado | 37 / 12 / 6 |
| Warnings de victor count | 0 |

Conflictos de nivel:

- Victors 36, **Erebus**, rank histórico 426: sin coincidencia exacta. Candidatos
  SQL: IdNivel 429, `Erebus (BoldStep)` (posición actual 426) y 680,
  `ErebuS (Platnuu)` (677). Ninguno se vincula por posición.
- Victors 49, **2‎ 1‎ 1**, rank 539: marcas invisibles U+200E. Sugerencia SQL:
  IdNivel 542, `2 1 1` (539), pendiente de revisión.
- Victors 134, **Heartbeat**, rank 1070: sin coincidencia exacta. Sugerencia SQL:
  IdNivel 1073, `Heartbeat (KrmaL)` (1070), pendiente de revisión.

Conflictos de jugador:

- `z` en The Hell Inferno (fila 64).
- 32 nombres terminados en punto en Cataclysm (fila 221); el TXT/JSON detalla
  cada nombre y su sugerencia en Players. No se eliminaron puntos automáticamente.
- `Bruno` en Cataclysm (fila 221).
- `Okrun` en Cataclysm (fila 223).
- `ThaLion087` en Niwa y Acu (filas 241 y 250).

Warnings encontrados (7): AvatarUrl ausente en SQL; Fif repetido en Astral
Divinity; Inc0gnit repetido en Aftermath; WinRAR y Neuronfish repetidos en la
segunda fila de Cataclysm; fila de Cataclysm repetida; Santiricca repetido en
Rauchkammer. Los counts coinciden con celdas no vacías, no con jugadores únicos.

Detalle completo y revisable: `import-data/reports/udl-dry-run.txt` y `.json`.
Incluyen todos los candidatos SQL, jugadores y 819 decisiones de records.

## Verificación

- Release: cero errores y advertencias.
- 37 checks del importador aprobados, además de todos los checks existentes AREDL.
- Rechazo comprobado fuera de Development y sin `--dry-run` (código 1).
- `sql/verify-udl-import-readonly.sql` calcula conteos y hashes de todas las filas
  de Jugador, Nivel, Submission, Record, HistorialNivel y metadatos de columnas.
  Se ejecutó antes y después. `sql-before.txt` y `sql-after.txt` son idénticos:
  SHA-256 del archivo `C36D7779F1A7CE89587E596EF1489EF7F715878EFECD643E21F2465B33853218`.
- SQL continúa con 0 jugadores, 1606 niveles, 0 submissions, 0 records y 1606
  historiales; ninguna columna fue modificada por esta tarea.

## Archivos

Modificados: `Program.cs`, `Models/Jugador.cs`, `Models/Record.cs`,
`Data/UdlDbContext.cs`, `README.md`.

Creados: `Services/Import/ImportNames.cs`, `ImportPlan.cs`,
`UdlSheetSnapshot.cs`, `UdlImportPlanner.cs`, `UdlImportDatabaseReader.cs`,
`UdlImportReport.cs`; `tests/UDL.Import.Checks/Program.cs` y `.csproj`;
los dos scripts `sql/`; snapshot e informes `import-data/`; este documento.
