using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace ClassBook.Shared.Security;

/// <summary>
/// CSRF protection for the cookie-authenticated SPA. Safe API requests receive the readable <c>XSRF-TOKEN</c> cookie;
/// unsafe API requests must echo it in the <c>X-XSRF-TOKEN</c> header, or get <c>400</c>.
/// </summary>
internal sealed class AntiforgeryMiddleware(RequestDelegate next, IAntiforgery antiforgery)
{
    public const string CookieName = "XSRF-TOKEN";
    public const string HeaderName = "X-XSRF-TOKEN";
    public const string InvalidTokenCode = "antiforgery.invalid";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method) || HttpMethods.IsOptions(context.Request.Method))
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            context.Response.Cookies.Append(CookieName, tokens.RequestToken!, new CookieOptions
            {
                HttpOnly = false, // The SPA reads it and sends it back in the header.
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/",
            });
            await next(context);
            return;
        }

        if (!await antiforgery.IsRequestValidAsync(context))
        {
            await Results.Problem(
                    detail: "Missing or invalid antiforgery token.",
                    statusCode: StatusCodes.Status400BadRequest,
                    extensions: new Dictionary<string, object?> { ["code"] = InvalidTokenCode })
                .ExecuteAsync(context);
            return;
        }

        await next(context);
    }
}
