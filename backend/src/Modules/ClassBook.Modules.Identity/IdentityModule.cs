using ClassBook.Modules.Identity.Domain;
using ClassBook.Modules.Identity.Persistence;
using ClassBook.Shared.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ClassBook.Modules.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddModuleDbContext<IdentityDbContext>();

        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
        services.AddIdentityCore<AppUser>(o =>
            {
                o.User.RequireUniqueEmail = true;
                o.Password.RequiredLength = 10;
                o.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(o =>
        {
            o.Cookie.HttpOnly = true;
            o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            o.Cookie.SameSite = SameSiteMode.Lax;
            // The SPA handles navigation; the API answers with status codes instead of redirects.
            o.Events.OnRedirectToLogin = context => WriteStatus(context.Response, StatusCodes.Status401Unauthorized);
            o.Events.OnRedirectToAccessDenied = context => WriteStatus(context.Response, StatusCodes.Status403Forbidden);
        });

        return services;
    }

    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app) => app;

    private static Task WriteStatus(HttpResponse response, int statusCode)
    {
        response.StatusCode = statusCode;
        return Task.CompletedTask;
    }
}
