using Microsoft.AspNetCore.Mvc;
using UniversityCafeteria.Models;
using UniversityCafeteria.Data;
using Microsoft.EntityFrameworkCore;
namespace UniversityCafeteria.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var products = _context.products.ToList();
            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var categories = _context.categories.ToList();
            ViewBag.Categories = categories;

            var suppliers = _context.suppliers.ToList();
            ViewBag.Suppliers = suppliers;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product)
        {
            _context.products.Add(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var product = _context.products.Find(id);
           
            var categories = _context.categories.ToList();
            ViewBag.Categories = categories;

            var suppliers = _context.suppliers.ToList();
            ViewBag.Suppliers = suppliers;

            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            _context.products.Update(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var product = _context.products.Find(id);

            _context.products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
