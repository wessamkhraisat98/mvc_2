using Microsoft.AspNetCore.Mvc;
using UniversityCafeteria.Models;
using UniversityCafeteria.Data;
using Microsoft.EntityFrameworkCore;

namespace UniversityCafeteria.Controllers
{
    public class SupplierController : Controller
    {
        private readonly AppDbContext _context;

        public SupplierController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var suppliers = _context.suppliers.Include(s => s.Products).ToList();
            return View(suppliers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Supplier supplier)
        {
            _context.suppliers.Add(supplier);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var supplier = _context.suppliers.Find(id);
            return View(supplier);
        }

        [HttpPost]
        public IActionResult Edit(int id, Supplier supplier)
        {
            _context.suppliers.Update(supplier);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var supplier = _context.suppliers.Find(id);
            _context.suppliers.Remove(supplier);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
