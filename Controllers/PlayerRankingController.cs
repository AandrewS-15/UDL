using Microsoft.AspNetCore.Mvc;
using UDL.Services.Ranking;
using UDL.ViewModels;

namespace UDL.Controllers;

[ApiController]
[Route("api/players/ranking")]
public sealed class PlayerRankingController(PlayerRankingQuery ranking) : ControllerBase
{
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public Task<IReadOnlyList<PlayerRankingViewModel>> Get(CancellationToken ct) => ranking.GetAsync(ct);
}
