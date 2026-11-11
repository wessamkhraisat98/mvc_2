using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Authorize]
public class SupplyRequestController(ApplicationDbContext db, UserManager<ApplicationUser> users) : Controller
{
    private const string Managers = "Admin,SuperAdmin";
    private string CurrentUserId => users.GetUserId(User)!;
    private bool CanReview => User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

    public async Task<IActionResult> Index()
    {
        var requests = db.SupplyRequests.Include(x => x.User).AsQueryable();
        if (!CanReview) requests = requests.Where(x => x.UserId == CurrentUserId);
        return View(await requests.OrderByDescending(x => x.RequestDate).ToListAsync());
    }

    public async Task<IActionResult> Details(int id)
    {
        var request = await FindRequest(id);
        if (request is null || (!CanReview && request.UserId != CurrentUserId)) return NotFound();
        return View(request);
    }

    [HttpGet]
    public Task<IActionResult> Create() => ShowForm(0, []);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(List<RequestLine> lines)
    {
        var items = await BuildItems(lines);
        if (!ModelState.IsValid) return await ShowForm(0, lines);

        db.SupplyRequests.Add(new SupplyRequest
        {
            UserId = CurrentUserId, RequestDate = DateTime.Now, Status = RequestStatus.Pending,
            Items = items, TotalPrice = items.Sum(x => x.Price * x.Quantity)
        });
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var request = await FindRequest(id);
        if (request is null || request.UserId != CurrentUserId) return NotFound();
        if (request.Status != RequestStatus.Pending) return Forbid();
        return await ShowForm(id, request.Items.Select(x =>
            new RequestLine { ProductId = x.ProductId, Quantity = x.Quantity }).ToList());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, List<RequestLine> lines)
    {
        var request = await FindRequest(id);
        if (request is null || request.UserId != CurrentUserId) return NotFound();
        if (request.Status != RequestStatus.Pending) return Forbid();
        var items = await BuildItems(lines);
        if (!ModelState.IsValid) return await ShowForm(id, lines);

        db.RequestItems.RemoveRange(request.Items);
        request.Items = items;
        request.TotalPrice = items.Sum(x => x.Price * x.Quantity);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var request = await db.SupplyRequests.FindAsync(id);
        if (request is null || request.UserId != CurrentUserId) return NotFound();
        if (request.Status != RequestStatus.Pending) return Forbid();
        db.SupplyRequests.Remove(request);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Managers), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, RequestStatus status)
    {
        if (!ModelState.IsValid || status is not (RequestStatus.Approved or RequestStatus.Rejected))
            return BadRequest();
        var request = await db.SupplyRequests.FindAsync(id);
        if (request is null) return NotFound();
        if (request.Status != RequestStatus.Pending) return Forbid();
        request.Status = status;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    private Task<SupplyRequest?> FindRequest(int id) => db.SupplyRequests
        .Include(x => x.User).Include(x => x.Items).ThenInclude(x => x.Product)
        .FirstOrDefaultAsync(x => x.Id == id);

    private async Task<IActionResult> ShowForm(int id, List<RequestLine> lines)
    {
        ViewBag.RequestId = id;
        ViewBag.Products = await db.Products.OrderBy(x => x.Name).ToListAsync();
        return View("Form", lines);
    }

    private async Task<List<RequestItem>> BuildItems(List<RequestLine> lines)
    {
        var selected = lines.Where(x => x.Quantity > 0).ToList();
        if (selected.Count == 0)
            ModelState.AddModelError("", "Enter a quantity for at least one product.");
        var ids = selected.Select(x => x.ProductId).Distinct().ToList();
        if (ids.Count != selected.Count)
            ModelState.AddModelError("", "Select each product only once.");

        var products = await db.Products.Where(x => ids.Contains(x.Id)).ToDictionaryAsync(x => x.Id);
        var items = new List<RequestItem>();
        foreach (var line in selected)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
                ModelState.AddModelError("", "A selected product no longer exists.");
            else if (line.Quantity > product.Quantity)
                ModelState.AddModelError("", $"Only {product.Quantity} units of {product.Name} are available.");
            else
                items.Add(new RequestItem { ProductId = product.Id, Quantity = line.Quantity, Price = product.Price });
        }

        if (items.Sum(x => x.Price * x.Quantity) > 9999999999999999.99m)
            ModelState.AddModelError("", "The request total is too large.");

        return items;
    }
}
