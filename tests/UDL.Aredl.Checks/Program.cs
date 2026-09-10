using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using UDL.Services.Aredl;

const string validJson = """
[{"id":"94fddf8f-5edf-4db6-8ba7-9106d5b67d08","name":"Example",
"position":null,"publisher_id":"9e66153c-3064-4628-b791-aa2a9c783abe",
"points":5000,"status":"MainList","requires_raw_footage":false,
"level_id":127323087,"two_player":true,"tags":["Wave",null],
"description":null,"song":null,"edel_enjoyment":42.5,"is_edel_pending":true,
"gddl_tier":null,"nlw_tier":null}]
""";

static AredlService Service(string json, HttpStatusCode status = HttpStatusCode.OK,
    string mediaType = "application/json") => new(new HttpClient(new StubHandler((request, _) =>
    {
        if (request.RequestUri != new Uri("https://example.test/v2/api/aredl/levels")
            || request.Method != HttpMethod.Get) throw new Exception("URL o método incorrectos.");
        return Task.FromResult(new HttpResponseMessage(status)
            { Content = new StringContent(json, Encoding.UTF8, mediaType) });
    })) { BaseAddress = new Uri("https://example.test/v2/api/") });

static async Task ExpectFailure(AredlService service, AredlFailure expected)
{
    try { await service.GetLevelsAsync(); }
    catch (AredlException ex) when (ex.Failure == expected) { return; }
    throw new Exception($"Se esperaba {expected}.");
}

var levels = await Service(validJson).GetLevelsAsync();
var level = levels.Single();
if (level.Position is not null || level.Song is not null || level.Tags[1] is not null
    || level.EdelEnjoyment != 42.5 || !level.TwoPlayer || !level.IsEdelPending
    || level.Points != 5000 || level.CompletedByUser is not null)
    throw new Exception("Mapeo incorrecto.");
if ((await Service("[]").GetLevelsAsync()).Count != 0) throw new Exception("Lista vacía inválida.");
foreach (var invalid in new[] { "null", "{}", "[null]", "{", "[{}]" })
    await ExpectFailure(Service(invalid), AredlFailure.InvalidResponse);
foreach (var field in new[] { "name", "tags", "status", "points" })
{
    var document = JsonNode.Parse(validJson)!;
    document[0]![field] = null;
    await ExpectFailure(Service(document.ToJsonString()), AredlFailure.InvalidResponse);
}
var missing = JsonNode.Parse(validJson)!;
missing[0]!.AsObject().Remove("two_player");
await ExpectFailure(Service(missing.ToJsonString()), AredlFailure.InvalidResponse);
await ExpectFailure(Service("unavailable", HttpStatusCode.ServiceUnavailable), AredlFailure.Http);
await ExpectFailure(Service("<html></html>", mediaType: "text/html"), AredlFailure.InvalidResponse);
await ExpectFailure(new AredlService(new HttpClient(new StubHandler((_, _) =>
    throw new HttpRequestException("offline"))) { BaseAddress = new Uri("https://example.test/") }),
    AredlFailure.Network);
await ExpectFailure(new AredlService(new HttpClient(new StubHandler((_, _) =>
    throw new TaskCanceledException("timeout"))) { BaseAddress = new Uri("https://example.test/") }),
    AredlFailure.Timeout);
using var cancellation = new CancellationTokenSource();
cancellation.Cancel();
try
{
    await Service(validJson).GetLevelsAsync(cancellation.Token);
    throw new Exception("Se esperaba cancelación del solicitante.");
}
catch (OperationCanceledException) { }
Console.WriteLine("OK: URL, DTO nullable, campos requeridos, JSON inválido, HTTP, red, timeout y cancelación.");

var mapped = AredlLevelMapping.Map(level, 1606);
mapped.IdNivel = 123;
mapped.Publisher = "Conservar publisher";
mapped.Verifier = "Conservar verifier";
var same = AredlLevelMapping.Map(level, 1606);
if (AredlLevelMapping.Apply(mapped, same) || AredlLevelMapping.PreviousState(mapped, same, DateTime.UtcNow) is not null)
    throw new Exception("No-op genera cambio/historial.");
same.Nombre = "Nombre actualizado";
if (AredlLevelMapping.PreviousState(mapped, same, DateTime.UtcNow) is not null
    || !AredlLevelMapping.Apply(mapped, same)) throw new Exception("Cambio descriptivo incorrecto.");
foreach (var changedField in new[] { "position", "points", "status" })
{
    var next = AredlLevelMapping.Map(level, 1606);
    if (changedField == "position") next.PosicionAredl = 10;
    if (changedField == "points") next.PuntosAredl = 42;
    if (changedField == "status") next.EstadoAredl = "Legacy";
    var target = AredlLevelMapping.Map(level, 1606);
    target.IdNivel = 123;
    var timestamp = DateTime.UtcNow;
    var history = AredlLevelMapping.PreviousState(target, next, timestamp)
        ?? throw new Exception("Falta historial.");
    if (history.IdNivel != 123 || history.PosicionAredl != target.PosicionAredl
        || history.PuntosAredl != target.PuntosAredl || history.EstadoAredl != target.EstadoAredl
        || history.Fecha != timestamp) throw new Exception("No se preservó estado anterior.");
    AredlLevelMapping.Apply(target, next);
    if (AredlLevelMapping.PreviousState(target, next, timestamp) is not null
        || AredlLevelMapping.Apply(target, next)) throw new Exception("Historial duplicado.");
}
if (mapped.Publisher != "Conservar publisher" || mapped.Verifier != "Conservar verifier"
    || mapped.Tags != "[\"Wave\",null]") throw new Exception("Campos ajenos o tags modificados.");
var precision = JsonNode.Parse(validJson)!;
precision[0]!["edel_enjoyment"] = 54.37662338;
precision[0]!["gddl_tier"] = 23.97669491525424;
var precisionDto = (await Service(precision.ToJsonString()).GetLevelsAsync()).Single();
var rounded = AredlLevelMapping.Map(precisionDto, 1606);
if (rounded.EdelEnjoyment != 54.38m || rounded.GddlTier != 23.98m
    || AredlLevelMapping.Apply(rounded, AredlLevelMapping.Map(precisionDto, 1606)))
    throw new Exception("Redondeo SQL no idempotente.");
var invalidSql = JsonNode.Parse(validJson)!;
invalidSql[0]!["name"] = new string('x', 201);
try
{
    AredlLevelMapping.Map((await Service(invalidSql.ToJsonString()).GetLevelsAsync()).Single(), 1606);
    throw new Exception("Se aceptó un nombre fuera del esquema SQL.");
}
catch (ArgumentException) { }
Console.WriteLine("OK: mapeo SQL, precisión, historial anterior, cambios descriptivos, idempotencia y conservación de campos.");

foreach (var (position, expected) in new (int, decimal)[]
{
    (1, 10000m), (43, 4519.3m), (75, 4000m), (76, 3500m), (108, 1588m),
    (150, 1000m), (151, 995.2m), (253, 613m), (1606, 10m)
})
    if (UdlPointsCalculator.Calculate(position, 1606) != expected)
        throw new Exception($"Puntos incorrectos en posición {position}.");
foreach (var (position, total) in new[] { (0, 1606), (-1, 1606), (1607, 1606), (1, 0), (1, -1) })
{
    try { UdlPointsCalculator.Calculate(position, total); throw new Exception("Entrada inválida aceptada."); }
    catch (ArgumentOutOfRangeException) { }
}
if (UdlPointsCalculator.Calculate(75, 75) != 4000m
    || UdlPointsCalculator.Calculate(150, 150) != 1000m
    || UdlPointsCalculator.Calculate(151, 151) != 10m)
    throw new Exception("Catálogos pequeños incorrectos.");
for (var position = 2; position <= 1606; position++)
    if (UdlPointsCalculator.Calculate(position, 1606) > UdlPointsCalculator.Calculate(position - 1, 1606))
        throw new Exception("La curva no es decreciente.");
var rankedJson = JsonNode.Parse(validJson)!;
rankedJson[0]!["position"] = 253;
var rankedDto = (await Service(rankedJson.ToJsonString()).GetLevelsAsync()).Single();
var ranked = AredlLevelMapping.Map(rankedDto, 1606);
if (ranked.PuntosAredl != 613m || mapped.PuntosAredl != 0m)
    throw new Exception("Puntos UDL o nivel sin posición incorrectos.");
rankedJson[0]!["points"] = -123;
var differentExternalPoints = AredlLevelMapping.Map(
    (await Service(rankedJson.ToJsonString()).GetLevelsAsync()).Single(), 1606);
if (AredlLevelMapping.Apply(ranked, differentExternalPoints)
    || AredlLevelMapping.PreviousState(ranked, differentExternalPoints, DateTime.UtcNow) is not null)
    throw new Exception("Los puntos externos afectan UDL.");
var largerCatalog = AredlLevelMapping.Map(rankedDto, 1700);
if (largerCatalog.PuntosAredl == ranked.PuntosAredl
    || AredlLevelMapping.PreviousState(ranked, largerCatalog, DateTime.UtcNow) is null
    || !AredlLevelMapping.Apply(ranked, largerCatalog)
    || AredlLevelMapping.Apply(ranked, largerCatalog))
    throw new Exception("Cambio de n no recalcula puntos con historial idempotente.");
rankedJson[0]!["position"] = 108;
var moved = AredlLevelMapping.Map((await Service(rankedJson.ToJsonString()).GetLevelsAsync()).Single(), 1700);
if (moved.PuntosAredl != 1588m
    || AredlLevelMapping.PreviousState(ranked, moved, DateTime.UtcNow) is null
    || !AredlLevelMapping.Apply(ranked, moved)
    || AredlLevelMapping.PreviousState(ranked, moved, DateTime.UtcNow) is not null)
    throw new Exception("Cambio conjunto de posición y puntos incorrecto.");
Console.WriteLine("OK: fórmula UDL, referencias n=1606, límites, entradas inválidas, monotonía, points externo ignorado y recálculo por n/posición.");

sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send)
    : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken) => send(request, cancellationToken);
}

