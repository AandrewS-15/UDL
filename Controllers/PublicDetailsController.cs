using Microsoft.AspNetCore.Mvc;
using UDL.Services.Levels;
using UDL.Services.Ranking;

namespace UDL.Controllers;

public sealed class PublicDetailsController(PlayerProfileService players, LevelDetailsService levels) : Controller
{
    [HttpGet("/players/{id:int:min(1)}")]
    public async Task<IActionResult> Player(int id, CancellationToken ct)
    {
        var model = await players.GetAsync(id, ct);
        if (model is not null) return View(model);
        Response.StatusCode = 404;
        return View("NotFound", "Jugador no encontrado");
    }

    [HttpGet("/levels/{id:int:min(1)}")]
    public async Task<IActionResult> Level(int id, CancellationToken ct)
    {
        var model = await levels.GetAsync(id, ct);
        if (model is not null) return View(model);
        Response.StatusCode = 404;
        return View("NotFound", "Nivel no encontrado");
    }
}
