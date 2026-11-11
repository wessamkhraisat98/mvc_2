using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Authorize]
public class ProductController(ApplicationDbContext db) : Controller
{
    private const string Managers = "Admin,SuperAdmin";

    public async Task<IActionResult> Index() => View(await db.Products.OrderBy(x => x.Name).ToListAsync());

    public async Task<IActionResult> Details(int id)
    {
        var product = await db.Products.FindAsync(id);
        return product is null ? NotFound() : View(product);
    }

    [Authorize(Roles = Managers), HttpGet]
    public IActionResult Create() => View("Form", new Product());

    [Authorize(Roles = Managers), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,Price,Quantity")] Product product)
    {
        if (!ModelState.IsValid) return View("Form", product);
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Managers), HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await db.Products.FindAsync(id);
        return product is null ? NotFound() : View("Form", product);
    }

    [Authorize(Roles = Managers), HttpPost, ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEdit(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        if (!await TryUpdateModelAsync(product, "", x => x.Name, x => x.Description, x => x.Price, x => x.Quantity))
            return View("Form", product);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Managers), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();
        if (await db.RequestItems.AnyAsync(x => x.ProductId == id))
            TempData["Error"] = "This product is used in a supply request and cannot be deleted.";
        else
        {
            db.Products.Remove(product);
            await db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
