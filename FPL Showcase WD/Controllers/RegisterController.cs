using FPL_Showcase_WD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FPL_Showcase_WD.Controllers;

[AllowAnonymous]
[EnableRateLimiting("AuthLimiter")]
public sealed class RegisterController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new RegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email
        };

        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        await userManager.AddToRoleAsync(user, "User");
        await signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "FantasyTeam");
    }
}