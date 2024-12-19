using BuditelPhonebook.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using BuditelPhonebook.Repositories;
using BuditelPhonebook.Services;

namespace BuditelPhonebook
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // Add User Secrets in development environment
            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>();
            }

            // Use the connection string from configuration
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddScoped<IPersonRepository, PersonRepository>();


            // We will add repositories and authentication below

            var googleConfig = builder.Configuration.GetSection("Authentication:Google");
            var allowedDomain = googleConfig["AllowedDomain"];
            var adminEmails = builder.Configuration.GetSection("AdminEmails").Get<string[]>() ?? new string[0];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddGoogle(options =>
            {
                options.ClientId = googleConfig["ClientId"];
                options.ClientSecret = googleConfig["ClientSecret"];
                options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");

                options.Events.OnCreatingTicket = ctx =>
                {
                    var allowedDomain = googleConfig["AllowedDomain"];
                    var adminEmails = builder.Configuration.GetSection("AdminEmails").Get<string[]>() ?? new string[0];

                    var email = ctx.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                    if (email == null || !email.EndsWith("@" + allowedDomain, StringComparison.OrdinalIgnoreCase))
                    {
                        // Redirect user to AccessDenied page
                        ctx.Response.Redirect("/Account/AccessDenied");

                        // Fail the context to prevent continuing with sign-in
                        ctx.Fail("Email domain not allowed.");

                        return Task.CompletedTask;
                    }

                    var claimsIdentity = (System.Security.Claims.ClaimsIdentity)ctx.Principal.Identity;
                    if (adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase))
                    {
                        claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin"));
                    }
                    else
                    {
                        claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "User"));
                    }

                    return Task.CompletedTask;
                };

            });

            builder.Services.AddAuthorization();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();


            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                // Security headers
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' https://cdn.jsdelivr.net; style-src 'self' https://cdn.jsdelivr.net;");
                await next();
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Phonebook}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
