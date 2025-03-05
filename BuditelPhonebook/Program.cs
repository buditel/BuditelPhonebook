using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Core.Repositories;
using BuditelPhonebook.Core.Services;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
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


            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Use the connection string for DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                                options.UseSqlServer(connectionString));

            // Configure Identity
            //builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            // Add repositories
            builder.Services.AddScoped<IPersonRepository, PersonRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            builder.Services.AddScoped<IChangeLogRepository, ChangeLogRepository>();

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
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
            })
            .AddGoogle(options =>
            {
                options.ClientId = googleConfig["ClientId"];
                options.ClientSecret = googleConfig["ClientSecret"];
                options.CallbackPath = new PathString("/signin-google"); // Ensure it matches the URI registered in Google Console
                options.SaveTokens = true; // Save tokens to ensure proper state handling
                options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");

                options.Events.OnRemoteFailure = context =>
                {
                    // This handles the failed authentication scenario (like a correlation error)
                    context.Response.Redirect("/Account/Login");
                    context.HandleResponse();

                    return Task.CompletedTask;
                };

                options.Events.OnCreatingTicket = async ctx =>
                {
                    using var scope = ctx.HttpContext.RequestServices.CreateScope();
                    var userRoleRepository = scope.ServiceProvider.GetRequiredService<IUserRoleRepository>();

                    var email = ctx.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                    // Ensure the email matches the allowed domain
                    if (string.IsNullOrEmpty(email) || !email.EndsWith($"@{allowedDomain}", StringComparison.OrdinalIgnoreCase))
                    {
                        ctx.Fail("Email domain not allowed.");

                        return;
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

                    return;
                };
            });

            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();


            app.UseHttpsRedirection();
            app.UseHsts();
            app.UseStaticFiles();
            app.UseStatusCodePagesWithReExecute("/Error/{0}");


            app.UseRouting();


            app.UseAuthentication();

            app.Use(async (context, next) =>
            {
                // Check if the user is authenticated
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var email = context.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;

                    if (string.IsNullOrEmpty(email) || !email.EndsWith($"@{allowedDomain}", StringComparison.OrdinalIgnoreCase))
                    {
                        // If email is invalid, sign out and redirect to Access Denied
                        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        context.Response.Redirect("/Account/AccessDenied");
                        return; // Stop further processing
                    }
                }

                // Continue to the next middleware
                await next.Invoke();
            });

            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<ApplicationDbContext>();

                dbContext.Database.Migrate();

                var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
                var seeder = new ExcelDataSeeder(dbContext, httpContextAccessor);

                try
                {
                    var filePath = Path.Combine(app.Environment.WebRootPath, "OrgChart.xlsx");
                    if (File.Exists(filePath))
                    {
                        seeder.SeedData(filePath).GetAwaiter().GetResult(); // Run synchronously
                    }
                    else
                    {
                        Console.WriteLine($"Seeding skipped: File '{filePath}' not found.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during seeding: {ex.Message}");
                }
            }

            //Security headers
            app.Use(async (context, next) =>
            {
                var headers = context.Response.Headers;

                if (!headers.ContainsKey("Content-Security-Policy"))
                {
                    headers.Add("Content-Security-Policy",
                        "default-src 'self'; script-src 'self' https://cdn.jsdelivr.net; style-src 'self' https://cdn.jsdelivr.net; img-src 'self' https://buditel.softuni.bg data:;");
                }

                await next();
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Phonebook}/{action=Index}/{id?}");

            app.Run();
        }
    }
}