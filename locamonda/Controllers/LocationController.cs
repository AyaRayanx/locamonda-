using locamonda.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    public class LocationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── Index ─────────────────────────────

        public IActionResult Index()
        {
            var locations = _context.Locations.ToList();
            return View(locations);
        }

        // ─── Create GET ────────────────────────

        public IActionResult Create()
        {
            return View();
        }

        // ─── Create POST ───────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Location location)
        {
            if (ModelState.IsValid)
            {
                _context.Locations.Add(location);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(location);
        }

        // ─── Edit GET ──────────────────────────

        public IActionResult Edit(int id)
        {
            var location = _context.Locations.Find(id);
            if (location == null) return NotFound();

            return View(location);
        }

        // ─── Edit POST ─────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Location updatedLocation)
        {
            if (ModelState.IsValid)
            {
                var location = _context.Locations
                    .FirstOrDefault(l => l.LocationId == updatedLocation.LocationId);

                if (location == null) return NotFound();

                location.Country = updatedLocation.Country;
                location.City = updatedLocation.City;
                location.District = updatedLocation.District;
                location.ZipCode = updatedLocation.ZipCode;

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(updatedLocation);
        }

        // ─── Delete ────────────────────────────

        public IActionResult Delete(int id)
        {
            var location = _context.Locations.Find(id);
            if (location == null) return NotFound();

            _context.Locations.Remove(location);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}