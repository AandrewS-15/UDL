using Microsoft.AspNetCore.Identity;
using UDL.Data;
using UDL.Models;

namespace UDL.Services.Accounts;

public static class IdentitySetup
{
    public const string Usuario = "Usuario";
    public static readonly string[] Roles = [Usuario, "Moderador", "Administrador"];

    public static IServiceCollection AddUdlIdentity(this IServiceCollection services, bool development)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
            // Evita ambigüedad entre email y username durante el login.
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";
            options.Password.RequiredLength = 12;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.SignIn.RequireConfirmedEmail = false;
        }).AddEntityFrameworkStores<UdlDbContext>().AddDefaultTokenProviders();
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/account/login";
            options.AccessDeniedPath = "/account/denied";
            options.Cookie.Name = "UDL.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = development ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
        });
        services.AddScoped<PlayerAccountLinkService>();
        return services;
    }

    public static async Task SeedRolesAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var name in Roles)
        {
            if (await manager.RoleExistsAsync(name)) continue;
            var result = await manager.CreateAsync(new IdentityRole(name));
            if (!result.Succeeded && !await manager.RoleExistsAsync(name))
                throw new InvalidOperationException($"No se pudo crear el rol {name}.");
        }
    }
}
