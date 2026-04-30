using locamonda.Models;
using locamonda.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    public class PropertyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public PropertyController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        [Authorize(Roles = "Owner")]
        public IActionResult Create()
        {
            ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewBag.Amenities = _context.Amenities.ToList();

            return View();
        }

        // ─── Create POST ───────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Create(Property property, int[] amenityIds)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();

                property.OwnerId = user.Id;
                property.DateAdded = DateTime.Now;
                property.IsActive = true;

                _context.Properties.Add(property);
                await _context.SaveChangesAsync();

                if (amenityIds != null)
                {
                    foreach (var id in amenityIds)
                    {
                        _context.PropertyAmenities.Add(new PropertyAmenity
                        {
                            PropertyId = property.PropertyId,
                            AmenityId = id
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            ViewBag.Amenities = _context.Amenities.ToList();

            return View(property);
        }

        // ─── Edit GET ──────────────────────────

        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == id && p.OwnerId == user.Id);

            if (property == null) return NotFound();

            ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");

            return View(property);
        }

        // ─── Edit POST ─────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Edit(Property updatedProperty)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();

                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == updatedProperty.PropertyId && p.OwnerId == user.Id);

                if (property == null) return NotFound();

                property.Title = updatedProperty.Title;
                property.Description = updatedProperty.Description;
                property.Price = updatedProperty.Price;
                property.Rooms = updatedProperty.Rooms;
                property.Bathrooms = updatedProperty.Bathrooms;
                property.Status = updatedProperty.Status;
                property.LocationId = updatedProperty.LocationId;
                property.CategoryId = updatedProperty.CategoryId;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(updatedProperty);
        }

        // ─── Delete (soft) ─────────────────────

        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == id && p.OwnerId == user.Id);

            if (property == null) return NotFound();

            property.IsActive = false;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ─── My Properties ─────────────────────

        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> MyProperties()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var properties = await _context.Properties
                .Include(p => p.Location)
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .Where(p => p.OwnerId == user.Id)
                .ToListAsync();

            return View(properties);
        }
    }
}
