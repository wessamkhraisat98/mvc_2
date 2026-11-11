using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Authorize(Roles = "SuperAdmin")]
public class AdminController(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles,
    ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var accounts = await users.Users.OrderBy(x => x.Email).ToListAsync();
        var entries = new List<(ApplicationUser Account, string Roles)>();
        foreach (var account in accounts)
            entries.Add((account, string.Join(", ", await users.GetRolesAsync(account))));
        return View(entries);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var account = await users.FindByIdAsync(id);
        return account is null ? NotFound() : await ShowEdit(account);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, string roleId)
    {
        var account = await users.FindByIdAsync(id);
        if (account is null) return NotFound();
        var role = string.IsNullOrEmpty(roleId) ? null : await roles.FindByIdAsync(roleId);
        if (role?.Name is not ("User" or "Admin" or "SuperAdmin"))
            ModelState.AddModelError("", "Select a valid role.");
        else if (users.GetUserId(User) == id && role.Name != "SuperAdmin")
            ModelState.AddModelError("", "You cannot remove your own SuperAdmin role.");
        else
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            var current = await users.GetRolesAsync(account);
            var result = await users.RemoveFromRolesAsync(account, current.Where(x => x != role.Name));
            if (result.Succeeded && !current.Contains(role.Name))
                result = await users.AddToRoleAsync(account, role.Name);
            if (result.Succeeded)
                result = await users.UpdateSecurityStampAsync(account);
            if (result.Succeeded)
            {
                await transaction.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            await transaction.RollbackAsync();
        }

        return await ShowEdit(account);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var account = await users.FindByIdAsync(id);
        if (account is null) return NotFound();
        if (users.GetUserId(User) == id)
            TempData["Error"] = "You cannot delete your own account.";
        else
        {
            var result = await users.DeleteAsync(account);
            if (!result.Succeeded)
                TempData["Error"] = string.Join(" ", result.Errors.Select(x => x.Description));
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ShowEdit(ApplicationUser account)
    {
        ViewBag.Roles = await roles.Roles.OrderBy(x => x.Name).ToListAsync();
        ViewBag.CurrentRoles = await users.GetRolesAsync(account);
        return View("Edit", account);
    }
}
