using locamonda.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    public class PropertyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PropertyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── Index ─────────────────────────────

        public IActionResult Index(string city, decimal? minPrice, decimal? maxPrice, int? categoryId)
        {
            var properties = _context.Properties
                .Include(p => p.Location)
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .Where(p => p.IsActive && p.Status == "Available")
                .AsQueryable();

            if (!string.IsNullOrEmpty(city))
                properties = properties.Where(p => p.Location.City.Contains(city));

            if (minPrice.HasValue)
                properties = properties.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                properties = properties.Where(p => p.Price <= maxPrice.Value);

            if (categoryId.HasValue)
                properties = properties.Where(p => p.CategoryId == categoryId.Value);

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");

            return View(properties.ToList());
        }

        // ─── Details ───────────────────────────

        public IActionResult Details(int id)
        {
            var property = _context.Properties
                .Include(p => p.Location)
                .Include(p => p.Category)
                .Include(p => p.Owner)
                .Include(p => p.Photos)
                .Include(p => p.Reviews)
                .Include(p => p.PropertyAmenities).ThenInclude(pa => pa.Amenity)
                .FirstOrDefault(p => p.PropertyId == id);

            if (property == null) return NotFound();

            return View(property);
        }

        // ─── Create GET ────────────────────────

        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("AccountType");
            if (role != "Owner")
                return RedirectToAction("Login", "Users");

            ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewBag.Amenities = _context.Amenities.ToList();

            return View();
        }

        // ─── Create POST ───────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Property property, int[] amenityIds)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("AccountType");

            if (userIdStr == null || role != "Owner")
                return RedirectToAction("Login", "Users");

            if (ModelState.IsValid)
            {
                property.OwnerId = int.Parse(userIdStr);
                property.DateAdded = DateTime.Now;
                property.IsActive = true;

                _context.Properties.Add(property);
                _context.SaveChanges();

                foreach (var id in amenityIds)
                {
                    _context.PropertyAmenities.Add(new PropertyAmenity
                    {
                        PropertyId = property.PropertyId,
                        AmenityId = id
                    });
                }

                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewBag.Amenities = _context.Amenities.ToList();

            return View(property);
        }

        // ─── Edit GET ──────────────────────────

        public IActionResult Edit(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null) return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            var property = _context.Properties
                .FirstOrDefault(p => p.PropertyId == id && p.OwnerId == userId);

            if (property == null) return NotFound();

            ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");

            return View(property);
        }

        // ─── Edit POST ─────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Property updatedProperty)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null) return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            if (ModelState.IsValid)
            {
                var property = _context.Properties
                    .FirstOrDefault(p => p.PropertyId == updatedProperty.PropertyId && p.OwnerId == userId);

                if (property == null) return NotFound();

                property.Title = updatedProperty.Title;
                property.Description = updatedProperty.Description;
                property.Price = updatedProperty.Price;
                property.Rooms = updatedProperty.Rooms;
                property.Bathrooms = updatedProperty.Bathrooms;
                property.Status = updatedProperty.Status;
                property.LocationId = updatedProperty.LocationId;
                property.CategoryId = updatedProperty.CategoryId;

                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View(updatedProperty);
        }

        // ─── Delete (soft) ─────────────────────

        public IActionResult Delete(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null) return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            var property = _context.Properties
                .FirstOrDefault(p => p.PropertyId == id && p.OwnerId == userId);

            if (property == null) return NotFound();

            property.IsActive = false;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ─── My Properties ─────────────────────

        public IActionResult MyProperties()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null) return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            var properties = _context.Properties
                .Include(p => p.Location)
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .Where(p => p.OwnerId == userId)
                .ToList();

            return View(properties);
        }
    }
}