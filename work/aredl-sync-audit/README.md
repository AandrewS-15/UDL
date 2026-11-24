# Diagnóstico y sincronización AREDL

Se consultó https://api.aredl.net/v2/api/aredl/levels y se ejecutó exactamente dos veces el servicio existente, sin modificar su implementación.

Comando desde fuentes:
```powershell
dotnet run --project UDL.csproj --no-launch-profile -- --environment Development --sync-aredl
```
Comando ejecutado con el binario Release ya compilado:
```powershell
dotnet bin/Release/net10.0/UDL.dll --environment Development --sync-aredl
```

## Primera ejecución

- Catálogo: 1606 → 1615 niveles; 9 nuevos y ninguno local ausente en respuesta.
- Posición: 1479 cambios; puntos: 784 cambios; estado: 0 cambios. Los conteos se solapan.
- 1497 existentes actualizados; 109 sin cambios de contenido.
- 1481 historiales nuevos; corresponden exactamente, una vez cada uno, al estado anterior de los niveles que cambiaron posición/puntos/estado. Los otros 16 actualizados solo cambiaron datos descriptivos.
- Cero errores. UdlPointsCalculator coincide con los 1615 puntos SQL usando n=1615; no se usa el campo points de AREDL.

| Nivel | Posición anterior | Nueva | Puntos anteriores | Nuevos |
|---|---:|---:|---:|---:|
| UNKNOWN |126|127|1215.6|1201.8|
| Zodiac |164|165|935.5|931.5|
| Thinking Space |181|182|862.8|859.5|
| Bloodlust |253|255|613|609.1|
| Acu |1567|1576|10.2|10.2|

## Segunda ejecución

0 nuevos, 0 cambios de posición/puntos/estado, 0 historiales, 0 errores. El ranking queda idéntico al de la primera ejecución.

No fue un no-op total: 59 actualizados y 1556 sin cambios. La única diferencia de contenido entre snapshots fue EdelEnjoyment en 59 niveles (ejemplo: shimmer (amplitron), 60.93 → 61.28), procedente de la segunda respuesta remota. No se generó historial por ello. Además, por diseño existente, UltimaSincronizacion se escribe en todos los niveles recibidos en cada ejecución, aun sin cambios de contenido. No se ejecutó una tercera sincronización.

## Web y datos protegidos

La web de localhost:5083 ya estaba levantada y leyó los datos nuevos sin reinicio. Sus APIs de ranking (66 jugadores) y Lista Uruguay (250 niveles) coinciden con SQL. Las pruebas públicas contrastaron los 66 perfiles y 250 detalles con consultas SQL independientes. Se verificaron en navegador Bloodlust, su fila en Lista Uruguay, el perfil de Santiricca y ranking.

Totales: Santiricca 7918.8 → 7911.1; cxve 5483 → 5484.7; Fif 4820 → 4820.6. WinRAR pasa del puesto 28 al 27 y Thiago del 27 al 28. Los puntos se suman desde Nivel.PuntosAredl actual para Records al 100%, sin guardar valores congelados en Record.

Hashes de filas idénticos antes/después en Jugador, Record, Submission y las siete tablas Identity. Se conservan 66 jugadores, 781 Records y 0 submissions. No se ejecutó DDL ni se modificó schema. Se agregaron únicamente una utilidad de auditoría de solo lectura (`tests/UDL.SyncAudit.Checks`) y estos informes; no otro sistema de sincronización.

## Causa

Los niveles conservaban UltimaSincronizacion del 10/09/2026 21:33:27 UTC. Program.cs solo llama a SynchronizeAsync cuando recibe --sync-aredl en Development, antes de iniciar el servidor. No hay HostedService, temporizador ni tarea programada en el flujo actual. Abrir/reiniciar la web no consulta AREDL: muestra lo almacenado en SQL.

## Evidencia

- api-before.json: consulta inicial del catálogo.
- before.json, after-first.json, after-second.json: snapshots y hashes protegidos.
- sync-first.log, sync-second.log: resultados reales del servicio.
- comparison-first.txt, comparison-second.txt: fórmula/historial/conservación.
- second-field-diffs.json: diferencias descriptivas de la segunda ejecución.
- public-validation.txt y http-validation.json: validaciones SQL y localhost.
