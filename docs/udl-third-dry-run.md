# Tercer dry run: coincidencia exacta y fallback único

Se recuperaron los 17 niveles que habían quedado pendientes por capitalización.
No se ejecutó ALTER TABLE ni importación real; solo consultas SELECT y archivos
locales de informe. No se modificaron puntos, frontend, Identity ni submissions.

## Resolución vigente

1. Limpiar espacios y caracteres de formato, conservando capitalización.
2. Usar coincidencia exacta case-sensitive si es única.
3. Solo sin coincidencia exacta, buscar ignorando capitalización. Usar únicamente
   si hay un candidato; si hay varios, conflicto sin consultar aliases para ocultarlo.
4. Solo sin candidatos, consultar los aliases explícitos ya aprobados.
5. Mantener conflicto si el alias no existe o es ambiguo.

El catálogo SQL utiliza `Erebus (BoldStep)` y `ErebuS (Platnuu)`, por lo que en
esta base se conserva el alias explícito de Erebus a BoldStep. Los tests prueban
también que cuando existen `Erebus` y `ErebuS` literalmente, la coincidencia exacta
de Erebus gana antes del fallback case-insensitive. Heartbeat mantiene su alias
a Heartbeat (KrmaL); `2 1 1` se resuelve tras quitar caracteres de formato.

La resolución de jugadores no cambia. El informe ahora separa presencia en
Players, candidatos en Players y candidatos SQL. Las sugerencias tipográficas
de una sola edición para usernames de al menos cuatro caracteres son únicamente
informativas: no se utilizan para vincular ni crear jugadores.

## Corrección de avatares

La afirmación previa de que las celdas estaban vacías era incorrecta. Se verificó
`Players!B2:B64` con CellData (`userEnteredValue`, `effectiveValue`, `hyperlink`)
y con lectura FORMULA. Las **63 celdas contienen IMAGE con una URL literal**.
La lectura UNFORMATTED_VALUE usada antes no devuelve texto para esas imágenes.

Se agregó extracción limitada de URL literal desde IMAGE, sin evaluar fórmulas,
descargar imágenes ni generar URLs. Las URLs pasan la validación ya existente.
Algunas son avatares predeterminados de Discord que ya estaban en la planilla;
no fueron inventados por UDL. La captura nueva conserva las fórmulas originales
en la columna B para trazabilidad. No se verificó disponibilidad HTTP de imágenes.

## Fuente y reproducción

Captura real nueva, UTC 2026-09-10T22:45:48.368Z:
`import-data/udl-sheet.third.snapshot.json`.
Se leyeron completas Players!A1:BH994 y Victors!A1:BO312, más las fórmulas de
Players!B2:B64. Para futuras capturas, leer también las fórmulas de avatar de
todas las filas de jugadores, no solo los valores efectivos.

```powershell
dotnet run -c Release --no-build -- --import-udl-sheet --dry-run --sheet-snapshot import-data/udl-sheet.third.snapshot.json --report-prefix import-data/reports/udl-third-dry-run
```

## Resultado completo

| Concepto | Resultado |
|---|---:|
| Jugadores detectados / nuevos / existentes | 63 / 63 / 0 |
| Conflictos en Players / ocurrencias en Victors | 0 / 5 |
| Con AvatarUrl / sin AvatarUrl | 63 / 0 |
| Filas de niveles / resueltas | 251 / 251 |
| No encontrados / ambiguos / otros conflictos | 0 / 0 / 0 |
| Completions detectadas | 819 |
| Records nuevos / existentes | 776 / 0 |
| Duplicados omitidos | 38 |
| Records omitidos totales | 43 |

No quedan conflictos de nivel. Los 43 omitidos son 38 duplicados más 5 conflictos
de jugador. Los duplicados de Aftermath y Rauchkammer ahora se clasifican como
duplicados en vez de omitidos por nivel sin resolver.

## Conflictos de jugador pendientes

Los nombres se muestran exactamente como aparecen en Victors. Ninguno aparece
en Players con las reglas de matching de usernames. No hay jugadores en SQL y,
por tanto, no hay candidatos SQL para ninguno.

| Nombre Victors | Fila | Completion asociada | AREDL histórico | Candidatos Players |
|---|---:|---|---:|---|
| z | 64 | The Hell Inferno, 100% | 653 | Ninguno |
| Bruno | 221 | Cataclysm, 100% | 1477 | Ninguno |
| Okrun | 223 | Cataclysm, 100% | 1477 | Okarun, solo sugerencia manual |
| ThaLion087 | 241 | Niwa, 100% | 1533 | Ninguno |
| ThaLion087 | 250 | Acu, 100% | 1567 | Ninguno |

Motivo en todos: victor ausente de Players y sin coincidencia única en SQL.
No se cambió Okrun por Okarun ni se creó ninguno de estos jugadores.

## Warnings y validación

- Victor count: 0 discrepancias.
- Filas duplicadas: 1, Cataclysm.
- Victors repetidos: 6 grupos: Fif (Astral Divinity), Inc0gnit (Aftermath),
  EmilioRory, WinRAR y Neuronfish (Cataclysm), Santiricca (Rauchkammer).
- Otro aviso: AvatarUrl aún no existe en SQL. No es un conflicto de importación;
  el lector lo trata como NULL y el ALTER sigue pendiente.
- Release: 0 errores y advertencias. Pasaron 60 checks del importador y todos
  los checks AREDL. Incluyen prioridad exacta, fallback único, ambigüedad,
  aliases, caracteres de formato, IMAGE y sugerencias de jugador sin vinculación.
- `sql-third-before.txt` y `sql-third-after.txt` son idénticos:
  SHA-256 `C36D7779F1A7CE89587E596EF1489EF7F715878EFECD643E21F2465B33853218`.
  Incluyen hashes de filas y columnas de las cinco tablas. SQL conserva 0 jugadores,
  1606 niveles, 0 submissions, 0 records y 1606 historiales.

El script `sql/prepare-udl-historical-import.sql` permanece sin cambios y sin
ejecutar. Conserva UNIQUE(IdJugador, IdNivel) y prepara AvatarUrl y la nulabilidad
de VideoUrl/IdSubmissionOrigen. No se creó evidencia ni submission ficticia.

Informes completos: `import-data/reports/udl-third-dry-run.txt` y `.json`.
Archivos de código modificados en esta revisión: `Services/Import/UdlImportPlanner.cs`,
`ImportPlan.cs`, `UdlImportReport.cs`, `UdlSheetSnapshot.cs` y
`tests/UDL.Import.Checks/Program.cs`. Se agregaron la captura, informes y este documento.
