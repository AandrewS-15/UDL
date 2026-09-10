using Microsoft.AspNetCore.Mvc;

namespace UDL.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
