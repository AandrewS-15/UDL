using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UDL.Models;
using UDL.Services.Submissions;
using UDL.ViewModels;

namespace UDL.Controllers;

[Authorize, AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SubmissionsController(SubmissionService service, UserManager<ApplicationUser> users) : Controller
{
    [HttpGet("/submissions/new")]
    public async Task<IActionResult> New(int? levelId, string? search, CancellationToken ct)
    {
        if (await service.PlayerAsync(users.GetUserId(User)!, ct) is null) return LinkRequired();
        var model = new CreateSubmissionViewModel { IdNivel = levelId ?? 0 };
        await service.PrepareAsync(model, search, ct);
        return View(model);
    }

    [HttpPost("/submissions/new")]
    public async Task<IActionResult> New(CreateSubmissionViewModel model, CancellationToken ct)
    {
        var userId = users.GetUserId(User)!;
        if (await service.PlayerAsync(userId, ct) is null) return LinkRequired();
        if (ModelState.IsValid)
        {
            var result = await service.CreateAsync(userId, model, ct);
            if (result.Id.HasValue)
            {
                TempData["SubmissionSent"] = "Tu record fue enviado y está pendiente de revisión.";
                return RedirectToAction(nameof(Mine));
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.MemberNames.FirstOrDefault() ?? "", error.ErrorMessage!);
        }
        await service.PrepareAsync(model, null, ct);
        return View(model);
    }

    [HttpGet("/submissions/mine")]
    public async Task<IActionResult> Mine(int page = 1, CancellationToken ct = default)
    {
        var model = await service.HistoryAsync(users.GetUserId(User)!, page, ct);
        return model is null ? LinkRequired() : View(model);
    }

    private ViewResult LinkRequired() { Response.StatusCode = 403; return View("LinkRequired"); }
}
