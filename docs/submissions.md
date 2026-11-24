# Envíos de records — primera etapa

## Flujo y rutas

- `GET /submissions/new`: autenticación y jugador vinculado; búsqueda local por nombre (hasta 20 resultados) y selección mediante `levelId`.
- `POST /submissions/new`: antiforgery, modelo limitado y validación de negocio. Guarda exclusivamente una Submission Pendiente y redirige a Mis envíos con confirmación.
- `GET /submissions/mine`: historial exclusivo del jugador vinculado, orden FechaEnvio/IdSubmission descendente y páginas de 50 filas.
- Sin vínculo: respuesta 403 con explicación y enlace a Cuenta. Sin sesión: Identity redirige al login.

El menú de cuenta muestra Enviar record y Mis envíos cuando hay vínculo. Los niveles permiten preseleccionar el nivel. Las rutas antiguas `#submit` y `#submissions` redirigen al flujo real. Se mantienen estilos y layout actuales.

## Arquitectura y archivos

- `Controllers/SubmissionsController.cs`: autorización, ModelState, antiforgery y respuestas MVC.
- `ViewModels/SubmissionViewModels.cs`: entrada sin campos sensibles, URL HTTP(S), resultados e historial.
- `Services/Submissions/SubmissionRules.cs`: reglas testeables, porcentajes y raw footage.
- `Services/Submissions/SubmissionService.cs`: consultas EF, asociación de Identity, creación transaccional y detección de duplicados.
- `Views/Submissions/New.cshtml`, `Mine.cshtml`, `LinkRequired.cshtml`: vistas Razor, texto codificado automáticamente, sin HTML arbitrario.
- `Program.cs`: registro del servicio.
- `Views/Shared/_AccountNav.cshtml`, `Views/PublicDetails/Level.cshtml`, `wwwroot/udl-navigation.js`, `wwwroot/udl-styles.css`: navegación y estilos mínimos.
- `tests/UDL.Submissions.Checks`: reglas y pruebas HTTP opcionales contra localhost:5083.

## Reglas y seguridad

Solo se aceptan completions al 100%, independientemente de Main, Extended o Legacy. Las reglas anteriores 50%/40%/1% quedaron retiradas del flujo. El formulario muestra Completion (100%) sin input editable ni oculto; una propiedad de entrada con valor predeterminado 100 permite rechazar requests que intenten enviar otro porcentaje. El servicio asigna la constante 100 al guardar. Posición nula o no positiva bloquea el envío. El servicio lee posición y RequiresRawFootage desde SQL en cada POST. Raw footage obligatorio cuando el nivel lo exige; de lo contrario vacío se guarda NULL.

Video obligatorio, URLs absolutas HTTP/HTTPS sin credenciales, máximo 500 caracteres. No se descargan videos ni se accede a URLs desde el servidor. FPS entero positivo opcional, dispositivo hasta 50 caracteres y notas hasta 500. Notas/dispositivo rechazan marcado con `<` o `>`; Razor codifica texto al mostrarlo.

IdJugador se obtiene de Identity, nunca del formulario. Estado Pendiente, FechaEnvio UTC, FechaRevision/IdModerador/MotivoRechazo NULL se asignan en servidor. El vínculo sigue siendo provisional: no acredita identidad ni concede permisos sobre Record.

Una transacción obtiene `sys.sp_getapplock` exclusivo por jugador, comprueba el duplicado pendiente de jugador+nivel+porcentaje e inserta. El bloqueo se libera al confirmar/revertir, evitando la carrera entre dos solicitudes sin agregar índices o tablas. La protección cubre escrituras de este servicio; cualquier futura vía de creación deberá reutilizarlo. Nuevos intentos tras resolver un envío pueden conservar su historial; los progresos parciales no se aceptan.

## Base de datos

No hay migración ni cambio de schema. Se utiliza Submission existente. El servicio no escribe Record, Nivel, Jugador, HistorialNivel ni eventos. Las consultas públicas existentes siguen basadas en Record aprobado y no reciben puntos de un envío pendiente.

## Pruebas

```powershell
dotnet run --project tests/UDL.Submissions.Checks -c Release
# Con la aplicación local en http://localhost:5083:
dotnet run --project tests/UDL.Submissions.Checks -c Release --no-build -- --http
```

Las pruebas HTTP crean dos cuentas temporales y vinculan dos jugadores libres, sin editar Jugador. Agregan dos niveles sintéticos identificados por GUID y marcador para comprobar raw footage obligatorio y posición NULL (estados ausentes hoy entre los niveles reales). Los envíos llevan el mismo marcador aleatorio; el bloque finally elimina solamente los envíos, niveles sintéticos y cuentas de esa ejecución. No se eliminan submissions previos ni se editan niveles existentes. Si el proceso termina abruptamente, inspeccionar el marcador antes de cualquier limpieza manual. SQL Server puede consumir números de identidad durante las pruebas; no se reinician secuencias.

Validación histórica de la etapa anterior: 26 comprobaciones de reglas y 83 comprobaciones acumuladas con HTTP/SQL. Todas las suites anteriores (AREDL, Import, Ranking, LevelList, Public, Home e Identity) pasaron. En navegador se comprobó búsqueda de Tartarus (#108), mínimo 40%, envío y confirmación Pendiente; el envío visual y su cuenta temporal se limpiaron posteriormente. No se implementó moderación.

Los informes de solo lectura antes/después están en `work/submissions-validation`. Moderación, aprobación/rechazo, creación de Record, OAuth y eventos quedan fuera de esta etapa.

## Actualización: solo completions

Submission estaba vacía antes del cambio. Se conservan Porcentaje en Submission y Record y sus constraints SQL. No se modifican las entidades, el schema, AredlSyncService ni UdlPointsCalculator. PlayerRankingQuery sigue sumando Nivel.PuntosAredl actual desde Records al 100%; no se congela ningún puntaje por completion.

Archivos de esta actualización: SubmissionRules.cs, SubmissionService.cs, SubmissionViewModels.cs, Views/Submissions/New.cshtml y Mine.cshtml, wwwroot/udl-navigation.js (reglas visibles), tests/UDL.Submissions.Checks/Program.cs y este documento. Pruebas: 100 válido y 99/80/50/1 inválidos en todas las categorías, formulario sin input Porcentaje, omisión del campo guarda 100, raw footage, Pendiente, duplicados, Record y ranking intactos. Evidencia antes/después: work/submissions-validation/completions-before.txt y completions-after.txt.


Resultado: compilación Release con 0 errores y 0 advertencias; todas las suites existentes aprobadas. 81 comprobaciones de reglas y 156 acumuladas con HTTP/SQL aprobadas, incluida igualdad del ranking antes/después. Limpieza completa de fixtures. Hash de verificación histórica antes/después idéntico: CA46116E091B453D813EE674016AF353AA000298129ECFC1961E9DC38A4AED9D.

