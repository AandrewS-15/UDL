using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using UDL.Data;
using UDL.Models;
using UDL.Services.Accounts;
using UDL.ViewModels;

namespace UDL.Controllers;

[AutoValidateAntiforgeryToken]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn,
    UdlDbContext db, PlayerAccountLinkService links) : Controller
{
    [HttpGet("/account/register"), AllowAnonymous]
    public IActionResult Register() => User.Identity?.IsAuthenticated == true ? RedirectToAction(nameof(Index)) : View(new RegisterViewModel());

    [HttpPost("/account/register"), AllowAnonymous]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction(nameof(Index));
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.UserName.Trim(), Email = model.Email.Trim() };
        await using var transaction = await db.Database.BeginTransactionAsync();
        try
        {
            var result = await users.CreateAsync(user, model.Password);
            if (result.Succeeded) result = await users.AddToRoleAsync(user, IdentitySetup.Usuario);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
                return View(model);
            }
            await transaction.CommitAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", "No se pudo registrar: el usuario o email ya está en uso.");
            return View(model);
        }
        await signIn.SignInAsync(user, isPersistent: false);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/account/login"), AllowAnonymous]
    public IActionResult Login(string? returnUrl) => User.Identity?.IsAuthenticated == true
        ? LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : Url.Content("~/account"))
        : View(new LoginViewModel { ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : null });

    [HttpPost("/account/login"), AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var login = model.Login.Trim();
        var user = login.Contains('@') ? await users.FindByEmailAsync(login) : await users.FindByNameAsync(login);
        if (user is not null)
        {
            var result = await signIn.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded) return LocalRedirect(Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl! : Url.Content("~/account"));
        }
        ModelState.AddModelError("", "No se pudo iniciar sesión. Revisá los datos o intentá más tarde.");
        return View(model);
    }

    [HttpPost("/account/logout"), Authorize]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return LocalRedirect(Url.Content("~/#home"));
    }

    [HttpGet("/account"), Authorize]
    public async Task<IActionResult> Index(string? search, CancellationToken ct)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        return View(await AccountModel(user, search, ct));
    }

    [HttpPost("/account/link"), Authorize]
    public async Task<IActionResult> Link(int idJugador, CancellationToken ct)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        if (!ModelState.IsValid || idJugador <= 0) return BadRequest();
        var result = await links.LinkAsync(user, idJugador, ct);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
        Response.StatusCode = 409;
        return View("Index", await AccountModel(user, null, ct));
    }

    [HttpPost("/account/unlink"), Authorize]
    public async Task<IActionResult> Unlink(CancellationToken ct)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        var result = await links.UnlinkAsync(user);
        if (result.Succeeded) return RedirectToAction(nameof(Index));
        foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
        Response.StatusCode = 409;
        return View("Index", await AccountModel(user, null, ct));
    }

    [HttpGet("/account/denied")]
    public IActionResult Denied() { Response.StatusCode = 403; return View(); }

    private async Task<AccountViewModel> AccountModel(ApplicationUser user, string? search, CancellationToken ct)
    {
        var query = (search ?? "").Trim();
        if (query.Length > 50) query = query[..50];
        var player = user.IdJugador.HasValue ? await db.Jugador.AsNoTracking().Where(p => p.IdJugador == user.IdJugador)
            .Select(p => new PlayerChoiceViewModel { IdJugador = p.IdJugador, NombreGD = p.NombreGd }).SingleOrDefaultAsync(ct) : null;
        var available = player is null && query.Length > 0 ? await db.Jugador.AsNoTracking()
            .Where(p => p.NombreGd.Contains(query) && !db.Users.Any(u => u.IdJugador == p.IdJugador))
            .OrderBy(p => p.NombreGd).Take(20)
            .Select(p => new PlayerChoiceViewModel { IdJugador = p.IdJugador, NombreGD = p.NombreGd }).ToListAsync(ct) : [];
        return new AccountViewModel { UserName = user.UserName!, Email = user.Email!, Player = player,
            Verified = user.VinculoJugadorVerificado, Search = query, AvailablePlayers = available };
    }
}
