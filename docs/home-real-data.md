# Home con datos reales y cambios propios de UDL

## Arquitectura actual
HomeService ejecuta dos consultas EF de solo lectura:
- TopPlayers reutiliza PlayerRankingQuery.Build y toma el top 3 con el orden del ranking.
- RecentRecords exige IdSubmissionOrigen NOT NULL y Porcentaje=100; ordena por FechaAprobacion DESC e IdRecord DESC y toma 3. Su fecha se presenta como aprobación.

RecentLevelChanges queda vacío (opción A). La Home no consulta HistorialNivel. Su ViewModel conserva únicamente un contrato vacío reservado, sin persistencia ni campos AREDL. La vista muestra: “Aún no hay cambios recientes en la lista uruguaya.” Se conservan título, Ver lista, contenedor, estilo y tamaño aproximado.

## Corrección conceptual
La implementación anterior confundía cambios de metadata AREDL con eventos de la Lista Uruguay. HistorialNivel registra exclusivamente estados reemplazados durante el sync AREDL. No representa entradas a UDL. No se modificaron HistorialNivel ni AredlSyncService.

## Futuro
Al implementar aprobación/moderación se definirá la persistencia de eventos UDL. El evento relevante será el primer Record uruguayo aprobado al 100% de un nivel que antes tenía cero. La comprobación y el registro del evento deberán ser atómicos para evitar duplicados ante aprobaciones concurrentes. No se crearán eventos retroactivos para los importados ni eventos por el desplazamiento de otros niveles. No se implementa nada de ese flujo en esta etapa.

## Estado actual
Top 3: Santiricca 7918.8, cxve 5483, Fif 4820. Records recientes reales: 0, con “Aún no hay actividad reciente.” Cambios UY: 0. Sin eventos ficticios ni fechas de importación utilizadas como eventos.

## Archivos modificados en esta corrección
- Services/Home/HomeService.cs: retirada de la consulta AREDL, cambios UY vacíos.
- ViewModels/HomeViewModel.cs: retirada de snapshots y cálculos de metadata AREDL del bloque UY.
- Views/Home/_Content.cshtml: estado vacío correcto, conservando título y enlace.
- tests/UDL.Home.Checks/Program.cs: exclusión de importados/progresos, cambios vacíos, top3 igual al ranking; interceptor de logs comprueba exactamente dos consultas y ninguna a HistorialNivel.
- docs/home-real-data.md: significado y preparación futura actualizados.

La integración de tests usa únicamente lecturas contra SQL. Evidencia de hashes antes/después en work/home-events-validation.

## Validación completada
Compilación Release: 0 errores y 0 advertencias. Pasan los seis conjuntos de tests. La prueba SQL de Home confirma dos consultas, ninguna a HistorialNivel, top3 intacto y ambos bloques de eventos/actividad vacíos. Navegador: texto exacto esperado, título y enlace conservados; TAURINE, Thanatophobia y foggy morning ausentes del bloque. Los hashes antes/después de datos y columnas son idénticos: CA46116E091B453D813EE674016AF353AA000298129ECFC1961E9DC38A4AED9D. Sin escrituras, schema ni eventos ficticios.
