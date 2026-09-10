# UDL · Etapa 3: backend e integración de lectura AREDL

ASP.NET Core MVC (.NET 10) con Entity Framework Core 10.0.11 y SQL Server.

## Ejecutar

Requiere el SDK de .NET 10 y SQL Server local con la base `UDL` existente.
Desde esta carpeta:

```powershell
dotnet restore
dotnet run
```

Abrir http://localhost:5080. MVC entrega `Views/Home/Index.cshtml` mediante
`HomeController`. El HTML conserva el prototipo, con adaptación de las URLs
de recursos a Razor y una URL base para mantener los recursos relativos.

## Organización

- `Controllers/`: controlador inicial, sin operaciones de negocio.
- `Views/Home/Index.cshtml`: interfaz existente, sin layout adicional.
- `wwwroot/`: CSS, JavaScript e imágenes copiados sin cambios del prototipo.
- `Models/`: Jugador, Nivel, Submission, Record e HistorialNivel.
- `Data/UdlDbContext.cs`: mapeo obtenido leyendo la base existente.
- `Data/UdlDbContext.Constraints.cs`: CHECK y defaults complementarios.
- `outputs/`: prototipo original intacto, incluyendo documentación y ZIP.
- `work/`: herramientas y verificaciones anteriores, conservadas.

## Conexión y verificación

`appsettings.json` usa `Server=localhost;Database=UDL;Integrated Security=True;Encrypt=True;TrustServerCertificate=True`.
Se conecta con la identidad de Windows del proceso; esa cuenta debe tener acceso
a UDL. `TrustServerCertificate=True` permite el certificado local de desarrollo.
En un despliegue futuro deberá configurarse el certificado correspondiente.
La cadena puede sobreescribirse con `ConnectionStrings__UDL`.

```powershell
dotnet run -- --verify-database
```

Este comando ejecuta un SELECT limitado por cada entidad, sin mostrar registros,
y termina. Verifica la conexión, columnas y materialización. No inserta ni actualiza
datos. El arranque normal no consulta la base; la interfaz sigue usando su demo.

No se agregaron migraciones, EnsureCreated, EnsureDeleted ni Migrate.
La carga manual y controlada del catálogo se documenta en la Etapa 3.2.2.
No ejecutar `dotnet ef database update` sobre esta base: el esquema existente es
la fuente de verdad y cualquier cambio futuro requiere revisión explícita.

## Modelo existente

Se respetan `dbo`, claves identity, restricciones UNIQUE como claves alternativas,
longitudes, nulabilidad, decimales, defaults nombrados, 11 CHECK y seis relaciones
sin borrado en cascada. `NombreGd` y `Fps` se mapean a `NombreGD` y `FPS`.

- Jugador y Nivel tienen múltiples Submission y Record.
- Nivel tiene múltiples HistorialNivel.
- Cada Record referencia una Submission de origen. SQL no hace único
  `IdSubmissionOrigen`, por lo que la navegación inversa es una colección.
- Record es único por `(IdJugador, IdNivel)`.
- `IdModerador` es `nvarchar(450)` nullable, sin FK: no se agregó Identity.
- `AredlId` es GUID; porcentajes son byte; puntos son decimal.

El frontend contiene datos ficticios con nombres, índices locales, etiquetas como
arrays, imágenes y estados de cuenta. No es un modelo persistente equivalente al
SQL: no se importaron esos datos ni se vinculó la demo a EF. `Tags` sigue siendo
texto nullable según SQL, y `FPS` admite NULL aunque el formulario de demo lo exige.
Estas diferencias quedan para la futura integración; no requieren cambiar la base
en esta etapa.

No se implementó autenticación, registro, submissions, moderación,
sincronización automática, ranking ni puntos en el backend. Las simulaciones ya existentes
siguen funcionando exclusivamente en el navegador y se reinician al recargar.

Referencia: [ingeniería inversa de EF Core](https://learn.microsoft.com/en-us/ef/core/managing-schemas/scaffolding/).

## Validación de esta adaptación

- Compilación correcta y consultas EF de solo lectura a las cinco tablas.
- Respuesta HTTP correcta en `/`, `/Home/Index` y `/index.html`.
- HTML servido equivalente al original salvo URLs y elemento base.
- Los 11 recursos CSS, JavaScript e imágenes son idénticos byte a byte al original
  y se entregan correctamente por HTTP; ambos JavaScript pasan `node --check`.
- Las pruebas anteriores `work/check_stage1.cjs` y `work/check_stage1_raw.cjs`
  fallan con el frontend original porque su DOM simulado no implementa `remove()`.
  No se modificaron esas pruebas ni el prototipo. No se realizó QA visual en navegador.

## Etapa 3.2 · Cliente público AREDL

`Services/Aredl/AredlService.cs` es la única capa que realiza llamadas a AREDL.
Se registra como cliente tipado con `AddHttpClient` en `Program.cs`, sin dependencia
de EF ni de las entidades SQL. `Dtos/Aredl/AredlLevelDto.cs` representa el contrato
externo con nombres JSON explícitos. No se transforman DTOs en `Nivel` ni se guardan.

`appsettings.json` configura `Aredl:BaseUrl` y `Aredl:TimeoutSeconds` (30 segundos).
La configuración se valida al iniciar. El servicio admite cancelación y distingue
errores HTTP, red, timeout y respuestas inválidas. No agrega reintentos ni llamadas
automáticas. Los campos obligatorios ausentes o nulos y un JSON que no es una lista
se rechazan; los campos adicionales desconocidos se toleran.

### Probar desde MVC

```powershell
dotnet run
```

En Development, abrir http://localhost:5080/diagnostics/aredl/levels.
Devuelve `{ "total": ..., "sample": [...] }` con los primeros cinco DTOs después
de deserializar la lista completa. Es una ruta temporal sin enlace en el frontend;
fuera de Development devuelve 404. En fallo remoto devuelve ProblemDetails 502,
o 504 si vence el tiempo de espera. Cancelar la petición cancela también la llamada.

Si Debug está ejecutándose, usar otra configuración y puerto sin interrumpirlo:

```powershell
dotnet run -c Release -- --urls http://localhost:5082
```

### Contrato documentado y respuesta real

Fuentes oficiales: [documentación](https://api.aredl.net/v2/docs) y
[OpenAPI JSON](https://api.aredl.net/v2/openapi.json).

- OpenAPI publica servidor `https://api.aredl.net/v2` y ruta `/api/aredl/levels`.
  Con nuestra base `https://api.aredl.net/v2/api`, la ruta relativa es `aredl/levels`.
  La URL final verificada es `https://api.aredl.net/v2/api/aredl/levels`; no se duplica `/api`.
- La respuesta es un array directo, sin envoltorio ni paginación documentada.
  Se usa la petición pública sin credenciales ni filtros; OpenAPI indica que
  pending y removed se excluyen por defecto.
- `id` y `publisher_id` son UUID (`Guid`), `points` es int32 y `song` es int32
  nullable. `publisher_id` no es el nombre del publisher ni el ID de Geometry Dash.
- `position`, `description`, `song`, `edel_enjoyment`, `gddl_tier` y `nlw_tier`
  admiten null/ausencia. `tags` es un array requerido que admite elementos null.
- `edel_enjoyment` y `gddl_tier` son double, y `nlw_tier` es string nullable.
  La respuesta real incluye decimales con más de dos cifras: no se redondean a los
  decimales de SQL, dado que esta etapa no persiste datos.
- Los estados documentados son Pending, MainList, Legacy y Removed; se preservan
  como texto para admitir futuros valores externos sin perder información.
- OpenAPI describe `LevelWithUserCompletionStatus`, que agrega el campo opcional
  `completed_by_user`. No apareció en la respuesta pública observada y el DTO lo
  conserva nullable. Esto es compatible con la documentación, no un error.
- `requires_raw_footage` se documenta para niveles pending; se respeta el booleano
  recibido. No se infiere a partir del puesto ni de la regla de la demo frontend.

No se encontraron discrepancias de tipos entre OpenAPI y la respuesta observada.

### Validación realizada

Se ejecutó MVC en Release, Development, puerto 5082. La llamada a la ruta temporal
respondió HTTP 200, con **1606 niveles deserializados** y muestra de Society,
Thinking Space II, Amethyst, Flamewall y Tidal Wave. La portada también respondió
200. El total y los nombres corresponden al momento de la prueba y pueden cambiar.
Compilación Release: cero errores y cero advertencias. No hubo acceso a SQL en
esta prueba de AREDL ni modificaciones del frontend.

Comprobaciones reproducibles sin API ni SQL, mediante HttpMessageHandler simulado:

```powershell
dotnet run --project tests/UDL.Aredl.Checks -c Release
```

Verifican URL/método, mapeo nullable, lista vacía, campos obligatorios, JSON inválido,
contenido no JSON, HTTP de error, fallo de red, timeout y cancelación del solicitante.

## Etapa 3.2.2 · Sincronización manual del catálogo

`AredlSyncService` obtiene el catálogo usando `AredlService`; `AredlLevelMapping`
valida y compara los campos persistibles. No hay llamadas HTTP en controllers,
vistas o en el mapeador. No hay endpoint de escritura ni ejecución automática.

Ejecutar manualmente en Development (el perfil local ya lo configura):

```powershell
dotnet run -c Release -- --sync-aredl
```

El proceso imprime `AredlSyncResult` como JSON y termina. Fuera de Development
rechaza el comando antes de acceder a AREDL o SQL. Un error da código de salida 1.
Para evitar el log detallado de comandos EF:

```powershell
dotnet run -c Release -- --Logging:LogLevel:Microsoft.EntityFrameworkCore=Warning --Logging:LogLevel:System.Net.Http.HttpClient=Warning --sync-aredl
```

### Reglas

- Identificación exclusiva por `AredlId`, con restricción UNIQUE existente.
- Se valida todo el lote antes de escribir: límites SQL, longitudes, IDs duplicados,
  números finitos y catálogo no vacío. Un error de validación cancela el lote completo.
- Una transacción incluye niveles e historial. Un bloqueo SQL `sp_getapplock` con
  propietario Transaction serializa las escrituras de este servicio entre procesos.
  Ante errores antes del commit, se revierte la transacción.
- Se insertan los IDs nuevos; se comparan todos los campos mapeados de los existentes.
  No se borran ni se marcan como retirados los niveles ausentes de la respuesta:
  el endpoint excluye algunos estados por defecto.
- `UltimaSincronizacion` se actualiza en UTC para cada nivel recibido, aunque no
  cambie su contenido. `SinCambios` significa contenido igual: en esas filas EF
  actualiza únicamente esa fecha. Los niveles ausentes permanecen intactos.
- `Tags` contiene un array JSON compacto, preservando orden, strings y elementos
  null de AREDL. Se compara esa representación consistente.
- EDEL y GDDL se redondean a dos decimales, AwayFromZero, **antes** de comparar,
  para respetar decimal(5,2) y no generar actualizaciones en cada ejecución.
- Publisher/Verifier quedan NULL en inserts y se conservan en updates. No se
  inventan nombres. `is_edel_pending` y `completed_by_user` no tienen columna SQL
  y no se persisten. No se altera el esquema.

### Historial

Si cambia posición, puntos o estado, se inserta **una** fila con los tres valores
anteriores y `Fecha` UTC del momento de sustitución. Luego se actualiza `Nivel`.
El estado vigente reside en `Nivel`; los anteriores se reconstruyen ordenando
`HistorialNivel` por Fecha e IdHistorial. Fecha expresa fin de vigencia observado,
no la fecha exacta del cambio en AREDL. No se genera historial por inserts,
cambios descriptivos ni comprobaciones sin cambios. No se pueden reconstruir
cambios externos ocurridos entre dos consultas ni el inicio de vigencia anterior
a la primera observación; el esquema no almacena esos datos.

### Inspección previa y resultados reales

Había tres niveles de prueba, ninguno con un AredlId presente en el catálogo recibido:

| IdNivel | Nombre | Relaciones previas |
| --- | --- | --- |
| 1 | Bloodbath | Submission 1, jugador 1, Pendiente |
| 2 | Nivel Main Prueba | Ninguna |
| 3 | Nivel Extended Prueba | Ninguna |

No había Record ni HistorialNivel. Se conservaron las tres filas y la submission,
incluyendo sus valores y fechas. Bloodbath de prueba tiene AredlId
`1007c145-97a5-41ae-8bed-4830f99725e0`; el oficial es
`7a68bd82-91d3-429b-933c-dbf58f49e5c4`. Comparten GeometryDashId 10565740.
No se fusionaron ni se reasignaron FKs. Una futura limpieza requiere decidir
explícitamente qué hacer con la submission de prueba.

| Ejecución real | Nuevos | Actualizados | Sin cambios | Errores | Historiales nuevos |
| --- | ---: | ---: | ---: | ---: | ---: |
| Primera | 1606 | 0 | 0 | 0 | 0 |
| Segunda consecutiva | 0 | 0 | 1606 | 0 | 0 |

Verificación SQL final: **1609 filas, 1609 AredlIds distintos, 1606 niveles reales
de la respuesta, tres de prueba, 1606 Tags JSON válidos, una submission pendiente,
cero Record y cero HistorialNivel**. No se eliminó ni recreó ningún objeto SQL.

Las comprobaciones `tests/UDL.Aredl.Checks` también cubren redondeo SQL, cambios
descriptivos, captura del estado anterior para cada campo histórico, ausencia de
historial repetido, límites de columnas y preservación de Publisher/Verifier.
Esas pruebas usan objetos en memoria, sin introducir datos de prueba en SQL.

## Puntos UDL según la planilla

`UdlPointsCalculator.Calculate(position, totalLevels)` implementa los tres tramos
de la fórmula de Uruguay Demon List con `Math.Exp` y redondeo a un decimal
`MidpointRounding.AwayFromZero`. `Nivel.PuntosAredl` conserva su nombre y su tipo
SQL, pero ahora almacena puntos UDL calculados; el campo externo `points` sigue
en el DTO y no interviene en el cálculo ni en la validación del mapeo SQL.

Cada sincronización cuenta las posiciones positivas de la respuesta completa de
AREDL para obtener `n`, sin usar el número de filas locales. Los niveles sin
posición no cuentan en `n` y reciben cero puntos. Las posiciones no positivas o
mayores que `n` cancelan el lote antes de escribir. Los puntos se recalculan para
todos los niveles recibidos, incluso si solo cambió `n`. Se reutilizan las reglas
existentes de comparación, historial, transacción y bloqueo; un cambio conjunto
de posición y puntos produce una sola entrada de historial anterior.

Con `n = 1606`, los resultados verificados son: 1 → 10000,0; 43 → 4519,3;
75 → 4000,0; 76 → 3500,0; 108 → 1588,0; 150 → 1000,0; 151 → 995,2;
253 → 613,0; 1606 → 10,0. Los tests también verifican entradas inválidas,
catálogos pequeños, monotonía, independencia de puntos externos y cambios de n.

Validación de esta corrección: compilación Release sin errores ni advertencias y
todos los checks aprobados. Primera sincronización real: 1606 actualizados,
1606 historiales, cero nuevos y cero errores. Segunda consecutiva: 1606 sin
cambios, cero actualizados, cero historiales y cero errores. Como antes,
`UltimaSincronizacion` se refresca también en un no-op de contenido.
No se crearon migraciones ni se modificó el esquema SQL.
