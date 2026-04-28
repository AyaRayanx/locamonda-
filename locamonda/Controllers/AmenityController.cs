using locamonda.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    public class AmenityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AmenityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── Index ───────────────────────────

        public IActionResult Index()
        {
            var amenities = _context.Amenities.ToList();
            return View(amenities);
        }

        // ─── Create ───────────────────────────

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Amenity amenity)
        {
            if (ModelState.IsValid)
            {
                _context.Amenities.Add(amenity);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(amenity);
        }

        // ─── Edit ───────────────────────────

        public IActionResult Edit(int id)
        {
            var amenity = _context.Amenities.Find(id);
            if (amenity == null) return NotFound();

            return View(amenity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Amenity updatedAmenity)
        {
            if (ModelState.IsValid)
            {
                var amenity = _context.Amenities.Find(updatedAmenity.AmenityId);
                if (amenity == null) return NotFound();

                amenity.Name = updatedAmenity.Name;
                amenity.Icon = updatedAmenity.Icon;

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(updatedAmenity);
        }

        // ─── Delete ───────────────────────────

        public IActionResult Delete(int id)
        {
            var amenity = _context.Amenities.Find(id);
            if (amenity == null) return NotFound();

            _context.Amenities.Remove(amenity);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}