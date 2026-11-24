# Lista Uruguay real

GET /api/levels/uruguay ejecuta una consulta EF de solo lectura: EXISTS Record con Porcentaje=100, COUNT de esos records, orden PosicionAredl/Nombre. Los puntos se leen directamente de Nivel.PuntosAredl. PosicionUy se asigna secuencialmente después de consultar; Categoria se deriva en el ViewModel. Sin persistencia ni N+1.

## Archivos
- Program.cs: registro del servicio.
- Controllers/UruguayLevelListController.cs: endpoint de lectura.
- Services/Levels/UruguayLevelListQuery.cs: consulta EF.
- ViewModels/UruguayLevelListItemViewModel.cs: datos y categoría derivada.
- Views/Home/Index.cshtml: carga del script.
- wwwroot/udl-level-list.js: renderizado, búsqueda y estados de carga/error.
- wwwroot/udl-app.js: retirada exclusivamente de list/rows simulados.
- wwwroot/udl-account.js: texto informativo actualizado sobre referencia AREDL.
- tests/UDL.LevelList.Checks: inclusión/exclusión, orden, desempate, victors, puntos, categorías y traducción SQL.

## Validación 2026-09-11
Compilación Release: cero errores y advertencias. Pasan UDL.LevelList.Checks, UDL.Aredl.Checks, UDL.Import.Checks (63 comprobaciones) y UDL.Ranking.Checks.

250 niveles, 781 completions. Se compararon TODOS los campos de TODAS las filas API contra SQL independiente con JOIN/GROUP BY/ROW_NUMBER: sin diferencias. UY #1 Sakupen Circles, menor posición AREDL completada: 43. Ejemplos: Bloodlust 3 victors, Cataclysm 43, Acu 30.

Vista localhost comprobada: 250 tarjetas; búsqueda bloodlust devuelve UY #15, AREDL #253, 613 puntos y 3 victors. Se mantienen CSS, navbar, colores, tipografía y estructura de tarjetas. No había filtros de categoría, por lo que no se agregaron. Al no existir imágenes de nivel en el modelo, las tarjetas mantienen su fondo CSS sin imágenes ficticias. La ruta futura /levels/{id} queda en data-detail-url, sin activar enlaces a detalles inexistentes.

No se modificó SQL: hashes antes/después de las filas de las cinco tablas y de sus columnas idénticos. SHA256 de ambos informes: CA46116E091B453D813EE674016AF353AA000298129ECFC1961E9DC38A4AED9D.
Evidencia local en work/level-list-validation: sql-before.txt, sql-after.txt, compare.sql, sql-list.json y api-list.json.
