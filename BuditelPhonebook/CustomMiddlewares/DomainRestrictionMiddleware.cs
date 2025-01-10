using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

public class DomainRestrictionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _allowedDomain;

    public DomainRestrictionMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _allowedDomain = configuration.GetSection("Authentication:Google:AllowedDomain").Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Bypass domain restriction for the Logout and Login endpoints
        if (context.Request.Path.StartsWithSegments("/Account/Logout") ||
            context.Request.Path.StartsWithSegments("/Account/Login"))
        {
            await _next(context);
            return;
        }

        // Check if the user is authenticated
        if (context.User.Identity.IsAuthenticated)
        {
            var email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            // If the email is invalid or doesn't match the allowed domain, sign out and redirect
            if (string.IsNullOrEmpty(email) || !email.EndsWith($"@{_allowedDomain}", StringComparison.OrdinalIgnoreCase))
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/Account/AccessDenied");
                return;
            }
        }

        // Continue with the next middleware
        await _next(context);
    }
}
