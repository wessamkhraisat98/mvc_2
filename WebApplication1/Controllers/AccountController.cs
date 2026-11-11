using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

public class AccountController(
    UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn,
    ApplicationDbContext db) : Controller
{
    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDTO register)
    {
        if (!ModelState.IsValid) return View(register);

        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = new ApplicationUser
        {
            FullName = register.FullName.Trim(), Email = register.Email, UserName = register.Email
        };
        var result = await users.CreateAsync(user, register.Password);
        if (result.Succeeded)
            result = await users.AddToRoleAsync(user, "User");

        if (result.Succeeded)
        {
            await transaction.CommitAsync();
            return RedirectToAction(nameof(Login));
        }

        foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
        return View(register);
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDTO login)
    {
        if (!ModelState.IsValid) return View(login);

        var user = await users.FindByEmailAsync(login.Email);
        if (user is not null &&
            (await signIn.PasswordSignInAsync(user, login.Password, login.RememberMe, false)).Succeeded)
            return RedirectToAction("Index", "Product");

        ModelState.AddModelError("", "Incorrect email or password.");
        return View(login);
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();
}
