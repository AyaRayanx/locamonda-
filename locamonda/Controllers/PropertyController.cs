using locamonda.Data;
using locamonda.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string PROPERTY_IMAGE_PATH = "images/property";

        public PropertyController(AppDbContext context, UserManager<Users> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        #region --- Helpers ---

        private async Task NotifyAdmins(string type, string message)
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = admin.Id,
                    Type = type,
                    NotificationMessage = message,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
            }
            await _context.SaveChangesAsync();
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, PROPERTY_IMAGE_PATH);
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
            
            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create)) 
            { 
                await file.CopyToAsync(stream); 
            }
            return "/" + PROPERTY_IMAGE_PATH + "/" + fileName;
        }

        #endregion

        #region --- Public Listings ---

        // All properties for customers
        public IActionResult Index(string city, decimal? minPrice, decimal? maxPrice, int? categoryId)
        {
            var properties = _context.Properties
                .Include(p => p.Location)
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .Where(p => p.IsActive && p.IsApproved && p.Status == "Available")
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(city))
            {
                properties = properties.Where(p => p.Location != null && p.Location.City == city);
            }

            if (minPrice.HasValue) properties = properties.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue) properties = properties.Where(p => p.Price <= maxPrice.Value);
            if (categoryId.HasValue) properties = properties.Where(p => p.CategoryId == categoryId.Value);

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            return View(properties.ToList());
        }

        // Single property details
        public IActionResult Details(int id)
        {
            var property = _context.Properties
                .Include(p => p.Location)
                .Include(p => p.Category)
                .Include(p => p.Owner)
                .Include(p => p.Photos)
                .Include(p => p.Reviews)
                .FirstOrDefault(p => p.PropertyId == id);

            if (property == null) return NotFound();

            return View(property);
        }

        #endregion

        #region --- Owner Management ---

        // List properties owned by the current user
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> MyProperties()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var properties = await _context.Properties
                .Include(p => p.Location)
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .Where(p => p.OwnerId == user.Id && p.IsActive)
                .ToListAsync();

            return View(properties);
        }

        // Create Property (GET)
        [Authorize(Roles = "Owner")]
        public IActionResult Create()
        {
            ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // Create Property (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Create(Property property, string CategoryName, string LocationName, List<IFormFile> imageFiles)
        {
            foreach (var key in new[] { "Owner", "Location", "Category", "Photos", "Favorites", "Bookings", "Reviews" })
                ModelState.Remove(key);

            if (!ModelState.IsValid)
            {
                ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
                return View(property);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            CategoryName = CategoryName?.Trim();
            LocationName = LocationName?.Trim();

            var location = await _context.Locations.FirstOrDefaultAsync(l => l.City == LocationName);
            if (location == null && !string.IsNullOrEmpty(LocationName))
            {
                location = new Location { City = LocationName };
                _context.Locations.Add(location);
                await _context.SaveChangesAsync();
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == CategoryName);
            if (category == null && !string.IsNullOrEmpty(CategoryName))
            {
                category = new Category { Name = CategoryName };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            if (location == null || category == null)
            {
                ModelState.AddModelError(string.Empty, "Please select or type a valid Location and Property Type.");
                ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
                return View(property);
            }

            property.OwnerId = user.Id;
            property.LocationId = location.LocationId;
            property.CategoryId = category.CategoryId;
            property.DateAdded = DateTime.Now;
            property.IsActive = true;
            property.IsApproved = false; 
            property.Status = "Available";

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            if (imageFiles != null && imageFiles.Count > 0)
            {
                foreach (var file in imageFiles)
                {
                    string photoUrl = await SaveImage(file);
                    _context.Photos.Add(new Photo { PropertyId = property.PropertyId, PhotoUrl = photoUrl, IsMain = (file == imageFiles.First()) });
                }
                await _context.SaveChangesAsync();
            }

            await NotifyAdmins("NewProperty", $"New property '{property.Title}' added by {user.UserName} and needs approval.");

            TempData["Success"] = "Property added successfully and is pending admin approval.";
            return RedirectToAction(nameof(MyProperties));
        }

        // Edit Property (GET)
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var property = await _context.Properties
                .Include(p => p.Location)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.PropertyId == id && p.OwnerId == user.Id);

            if (property == null) return NotFound();

            ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");

            return View(property);
        }

        // Edit Property (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Edit(Property updatedProperty, string LocationName, string CategoryName, List<IFormFile> imageFiles)
        {
            foreach (var key in new[] { "Owner", "Location", "Category", "Photos", "Favorites", "Bookings", "Reviews" })
                ModelState.Remove(key);

            if (!ModelState.IsValid)
            {
                ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
                return View(updatedProperty);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var property = await _context.Properties
               .Include(p => p.Photos)
               .FirstOrDefaultAsync(p => p.PropertyId == updatedProperty.PropertyId && p.OwnerId == user.Id);

            if (property == null) return NotFound();

            CategoryName = CategoryName?.Trim();
            LocationName = LocationName?.Trim();

            var location = await _context.Locations.FirstOrDefaultAsync(l => l.City == LocationName);
            if (location == null && !string.IsNullOrEmpty(LocationName))
            {
                location = new Location { City = LocationName };
                _context.Locations.Add(location);
                await _context.SaveChangesAsync();
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == CategoryName);
            if (category == null && !string.IsNullOrEmpty(CategoryName))
            {
                category = new Category { Name = CategoryName };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
            }

            // Update basic fields
            property.Title = updatedProperty.Title;
            property.Description = updatedProperty.Description;
            property.Price = updatedProperty.Price;
            property.Area = updatedProperty.Area;
            property.Rooms = updatedProperty.Rooms;
            property.Bathrooms = updatedProperty.Bathrooms;
            property.Status = updatedProperty.Status;
            property.LocationId = location?.LocationId ?? updatedProperty.LocationId;
            property.CategoryId = category?.CategoryId ?? updatedProperty.CategoryId;
            property.Latitude = updatedProperty.Latitude;
            property.Longitude = updatedProperty.Longitude;
            
            // Features
            property.HasWifi = updatedProperty.HasWifi;
            property.HasAC = updatedProperty.HasAC;
            property.HasHeating = updatedProperty.HasHeating;
            property.HasGym = updatedProperty.HasGym;
            property.HasGarden = updatedProperty.HasGarden;

            // Handle new photos
            if (imageFiles != null && imageFiles.Count > 0)
            {
                // Delete old photos from DB and File System
                foreach (var oldPhoto in property.Photos.ToList())
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, oldPhoto.PhotoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath)) System.IO.File.Delete(oldFilePath);
                    _context.Photos.Remove(oldPhoto);
                }

                // Add new ones
                foreach (var file in imageFiles)
                {
                    string photoUrl = await SaveImage(file);
                    _context.Photos.Add(new Photo { PropertyId = property.PropertyId, PhotoUrl = photoUrl, IsMain = (file == imageFiles.First()) });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Property updated successfully!";
            return RedirectToAction(nameof(MyProperties));
        }

        // Soft Delete (Archive)
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

            TempData["Success"] = "Property has been archived.";
            return RedirectToAction(nameof(MyProperties));
        }

        #endregion
    }
}
