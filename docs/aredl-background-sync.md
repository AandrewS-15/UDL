# Sincronización automática AREDL

## Arquitectura

`AredlSyncBackgroundService` es un BackgroundService nativo. Tras la espera inicial abre un scope DI, resuelve `IAredlSyncService` (la implementación existente `AredlSyncService`), espera su resultado y programa el próximo ciclo. No tiene lógica de mapeo, puntos ni escrituras propia. No necesita tráfico web ni JavaScript. El servidor debe permanecer ejecutándose; no recupera ciclos mientras estuvo apagado.

Los ciclos no se solapan dentro del worker. Se conserva `sp_getapplock` transaccional existente en AredlSyncService para excluir escrituras simultáneas de otras instancias o del comando manual. Las consultas HTTP pueden coincidir, pero las transacciones sobre Nivel/HistorialNivel se serializan. No se agregó infraestructura distribuida.

El worker captura errores, informa duración y espera el siguiente intervalo; la web sigue leyendo SQL. Cancelación de apagado se propaga a HTTP/EF y no se registra como fallo. Cada scope se libera. Los resultados sin errores producen una única línea de resumen por ciclo; el inicio queda en Debug y los errores se registran con ILogger. Se reduce el detalle de comandos EF a Warning para evitar miles de líneas SQL cada hora.

## Configuración final

En la sección Aredl ya existente:
```json
{
  "SyncEnabled": true,
  "SyncIntervalMinutes": 60,
  "SyncInitialDelaySeconds": 15
}
```

El intervalo se cuenta desde que termina cada intento (incluidos fallos). Cambiar appsettings o variables de entorno no requiere recompilar; reiniciar el proceso para aplicar los nuevos valores. Rangos validados: intervalo 1–10080 minutos y espera inicial 0–3600 segundos.

Desactivar en Development/tests web cuando se necesite una base inmóvil:
```powershell
dotnet run --project UDL.csproj --no-launch-profile -- --environment Development --Aredl:SyncEnabled false
```
También admite `Aredl__SyncEnabled=false` como variable de entorno. No se registra en entorno Testing ni para --sync-aredl, --import-udl-sheet, --verify-database o --grant-role. EF tooling no inicia el host web y no ejecuta el worker. Las pruebas de servicios no arrancan hosts ni tocan la API automáticamente.

## Comando manual conservado

```powershell
dotnet run --project UDL.csproj --no-launch-profile -- --environment Development --sync-aredl
```

La automatización no se inicia en este modo. La validación de catálogo, transacción, cálculo UDL e historial siguen dentro del mismo AredlSyncService. No cambia el comportamiento de campos descriptivos remotos ni de UltimaSincronizacion.

## Prueba rápida

```powershell
dotnet bin/Release/net10.0/UDL.dll --urls http://localhost:5083 --environment Development --Aredl:SyncIntervalMinutes 1 --Aredl:SyncInitialDelaySeconds 3
```

Es una sobrescritura temporal de argumentos: no modifica appsettings. Finalizada la prueba, detener esa instancia y arrancar sin esos dos argumentos para volver a 60 minutos/15 segundos.

## Archivos

- Services/Aredl/AredlSyncBackgroundService.cs: ciclo y logging.
- Services/Aredl/IAredlSyncService.cs: contrato de la implementación existente y punto de sustitución para pruebas.
- Services/Aredl/AredlSyncService.cs: únicamente declara la implementación del contrato; sincronización intacta.
- Services/Aredl/AredlOptions.cs, Program.cs, appsettings.json: opciones, validación y DI.
- tests/UDL.AredlBackground.Checks: reloj virtual, habilitación, intervalo, fallo/reintento, scopes, no solapamiento y cancelación; sin espera real de una hora.

No se modifica schema, Jugador, Record, Submission, cuentas, roles ni vínculos. Ranking y perfiles continúan sumando Nivel.PuntosAredl actual para Records al 100%. El historial solo guarda cambios de posición/puntos/estado. Las pruebas AREDL existentes cubren la fórmula, cambios descriptivos, historial e idempotencia; la auditoría de SQL comprueba las tablas protegidas.

Los logs y snapshots de validación se guardan en work/aredl-background-validation.
