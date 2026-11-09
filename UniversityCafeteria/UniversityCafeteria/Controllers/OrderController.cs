using Microsoft.AspNetCore.Mvc;
using UniversityCafeteria.Models;
using UniversityCafeteria.Data;

namespace UniversityCafeteria.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;

        public OrderController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var orders = _context.orders.ToList();
            return View(orders);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Order order)
        {
            _context.orders.Add(order);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var order = _context.orders.Find(id);
            return View(order);
        }

        [HttpPost]
        public IActionResult Edit(int id, Order order)
        {
            _context.orders.Update(order);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var order = _context.orders.Find(id);
            _context.orders.Remove(order);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var order = _context.orders.Find(id);
            return View(order);
        }



    }
}
