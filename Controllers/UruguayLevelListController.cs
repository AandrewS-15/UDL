using Microsoft.AspNetCore.Mvc;
using UDL.Services.Levels;
using UDL.ViewModels;

namespace UDL.Controllers;

[ApiController]
[Route("api/levels/uruguay")]
public sealed class UruguayLevelListController(UruguayLevelListQuery levels) : ControllerBase
{
    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public Task<IReadOnlyList<UruguayLevelListItemViewModel>> Get(CancellationToken ct) => levels.GetAsync(ct);
}
