using Microsoft.AspNetCore.Mvc;
using UDL.Services.Aredl;

namespace UDL.Controllers;

// Ruta temporal de diagnóstico. No modifica la interfaz ni consulta SQL.
[Route("diagnostics/aredl")]
public sealed class AredlDiagnosticsController(
    AredlService aredlService, IWebHostEnvironment environment,
    ILogger<AredlDiagnosticsController> logger) : ControllerBase
{
    [HttpGet("levels")]
    public async Task<IActionResult> Levels(CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment()) return NotFound();

        try
        {
            var levels = await aredlService.GetLevelsAsync(cancellationToken);
            return Ok(new { total = levels.Count, sample = levels.Take(5) });
        }
        catch (AredlException ex)
        {
            logger.LogWarning(ex, "Prueba AREDL fallida: {Failure}, HTTP remoto {Status}",
                ex.Failure, ex.UpstreamStatusCode);
            return Problem(statusCode: ex.Failure == AredlFailure.Timeout ? 504 : 502,
                title: "No se pudo obtener la lista de AREDL.", detail: ex.Message);
        }
    }
}
