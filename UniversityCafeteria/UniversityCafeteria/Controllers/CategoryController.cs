using Microsoft.AspNetCore.Mvc;
using UniversityCafeteria.Models;
using UniversityCafeteria.Data;
using Microsoft.EntityFrameworkCore;

namespace UniversityCafeteria.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var categories = _context.categories.Include(c => c.Products).ToList();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            _context.categories.Add(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _context.categories.Find(id);
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(int id, Category category)
        {
            _context.categories.Update(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

   
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var category = _context.categories.Find(id);
            _context.categories.Remove(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
