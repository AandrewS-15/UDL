using Microsoft.AspNetCore.Mvc;

namespace UDL.Controllers;

public class HomeController(UDL.Services.Home.HomeService home) : Controller
{
    public IActionResult Index() => View();

    [HttpGet("/home/content")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Content(CancellationToken ct) => PartialView("_Content", await home.GetAsync(ct));
}
