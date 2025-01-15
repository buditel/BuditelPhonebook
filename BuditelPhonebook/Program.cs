using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Core.Repositories;
using BuditelPhonebook.Core.Services;
using BuditelPhonebook.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllersWithViews();

            // Add User Secrets in development environment
            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>();
            }

            // Configure database context
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Configure Identity
            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add repositories
            builder.Services.AddScoped<IPersonRepository, PersonRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();

            // Google authentication
            var googleConfig = builder.Configuration.GetSection("Authentication:Google");
            var allowedDomain = googleConfig["AllowedDomain"];
            var superAdminEmails = builder.Configuration.GetSection("SuperAdminEmails").Get<string[]>() ?? new string[0];

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

                options.Events.OnCreatingTicket = async ctx =>
                {
                    using var scope = ctx.HttpContext.RequestServices.CreateScope();
                    var userRoleRepository = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();

                    var email = ctx.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                    // Ensure the email matches the allowed domain
                    if (string.IsNullOrEmpty(email) || !email.EndsWith($"@{allowedDomain}", StringComparison.OrdinalIgnoreCase))
                    {
                        ctx.Options.AccessDeniedPath = "/Account/AccessDenied";
                        ctx.HttpContext.Response.StatusCode = 403;
                    }

                    // Fetch roles dynamically from the database
                    var adminEmails = await userRoleRepository.GetAdminEmailsAsync();
                    var moderatorEmails = await userRoleRepository.GetModeratorEmailsAsync();

                    var claimsIdentity = (System.Security.Claims.ClaimsIdentity)ctx.Principal.Identity;

                    // Assign roles
                    if (superAdminEmails.Contains(email, StringComparer.OrdinalIgnoreCase))
                    {
                        claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "SuperAdmin"));
                    }
                    else if (adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase))
                    {
                        claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin"));
                    }
                    else if (moderatorEmails.Contains(email, StringComparer.OrdinalIgnoreCase))
                    {
                        claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Moderator"));
                    }
                    else
                    {
                        claimsIdentity.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "User"));
                    }
                };
            });

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseMiddleware<DomainRestrictionMiddleware>();
            app.UseAuthorization();

            // Security headers
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
                context.Response.Headers.Add("Content-Security-Policy",
                    "default-src 'self'; script-src 'self' https://cdn.jsdelivr.net; style-src 'self' https://cdn.jsdelivr.net; img-src 'self' https://buditel.softuni.bg data:;");
                await next();
            });

            app.UseStatusCodePages(context =>
            {
                if (context.HttpContext.Response.StatusCode == 403)
                {
                    context.HttpContext.Response.Redirect("/Account/AccessDenied");
                }
                return Task.CompletedTask;
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Phonebook}/{action=Index}/{id?}");

            app.Run();
        }
    }
}