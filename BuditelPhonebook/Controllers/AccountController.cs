using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BuditelPhonebook.Web.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login(string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, "Google");
        }

        public async Task<IActionResult> AccessDenied()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }

    }
}
