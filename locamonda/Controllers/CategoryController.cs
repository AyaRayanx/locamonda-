using locamonda.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── Index ─────────────────────────────

        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        // ─── Create GET ────────────────────────

        public IActionResult Create()
        {
            return View();
        }

        // ─── Create POST ───────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(category);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // ─── Edit GET ──────────────────────────

        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // ─── Edit POST ─────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category updatedCategory)
        {
            if (ModelState.IsValid)
            {
                var category = _context.Categories
                    .FirstOrDefault(c => c.CategoryId == updatedCategory.CategoryId);

                if (category == null) return NotFound();

                category.Name = updatedCategory.Name;
                category.Description = updatedCategory.Description;

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(updatedCategory);
        }

        // ─── Delete ────────────────────────────

        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();

            _context.Categories.Remove(category);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}