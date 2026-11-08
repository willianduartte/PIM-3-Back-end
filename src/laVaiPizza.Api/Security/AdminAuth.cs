using Microsoft.AspNetCore.Authentication;

namespace laVaiPizza.Api.Security;

/// <summary>
/// Login da area administrativa (paginas Razor): cookie, separado do JWT usado pela API.
/// O cookie e HttpOnly (o JavaScript nao le), SameSite=Strict (nao viaja em requisicao de outro site)
/// e as acoes de escrita exigem token antiforgery.
/// </summary>
public static class AdminAuth
{
    public const string Scheme = "AdminCookie";

    public static AuthenticationBuilder AddAdminCookie(this AuthenticationBuilder builder)
    {
        return builder.AddCookie(Scheme, options =>
        {
            options.Cookie.Name = ".lavai.admin";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Secure automatico quando o acesso e por HTTPS
            options.LoginPath = "/admin/conta/entrar";
            options.AccessDeniedPath = "/admin/conta/negado";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
            options.SlidingExpiration = true;

            // Chamadas fetch() da tela (polling) recebem 401/403 em vez de uma pagina de login em HTML.
            options.Events.OnRedirectToLogin = contexto =>
            {
                if (EhFetch(contexto.Request)) contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                else contexto.Response.Redirect(contexto.RedirectUri);
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = contexto =>
            {
                if (EhFetch(contexto.Request)) contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
                else contexto.Response.Redirect(contexto.RedirectUri);
                return Task.CompletedTask;
            };
        });
    }

    private static bool EhFetch(HttpRequest request)
        => string.Equals(request.Headers["X-Requested-With"], "fetch", StringComparison.OrdinalIgnoreCase);
}
