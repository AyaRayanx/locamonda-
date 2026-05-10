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

        // اضافه
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PropertyController(AppDbContext context, UserManager<Users> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
             //اضافه
            _webHostEnvironment = webHostEnvironment;
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
        public async Task<IActionResult> Create(Property property, string CategoryName, string LocationName, List<IFormFile> imageFiles)
        {
            ModelState.Clear();
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

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

            property.OwnerId = user.Id;
            property.LocationId = location?.LocationId ?? 0;
            property.CategoryId = category?.CategoryId ?? 0;
            property.DateAdded = DateTime.Now;
            property.IsActive = true;
            property.Status = "Available";

            _context.Properties.Add(property);
            await _context.SaveChangesAsync();

            if (imageFiles != null && imageFiles.Count > 0)
            {
                foreach (var file in imageFiles)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/property");
                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create)) { await file.CopyToAsync(stream); }
                    _context.Photos.Add(new Photo { PropertyId = property.PropertyId, PhotoUrl = "/images/property/" + fileName });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyProperties));
        }

        // ─── Edit GET ──────────────────────────
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

        // ─── Edit POST ─────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Edit(Property updatedProperty, string LocationName, string CategoryName, List<IFormFile> imageFiles)
        {
            ModelState.Remove("Owner");
            ModelState.Remove("Location");
            ModelState.Remove("Category");
            ModelState.Remove("Photos");
            ModelState.Remove("PropertyAmenities");
            ModelState.Remove("Favorites");
            ModelState.Remove("Bookings");
            ModelState.Remove("Reviews");
            ModelState.Remove("Messages");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();

                var property = await _context.Properties
                   .Include(p => p.Photos)
                   .FirstOrDefaultAsync(p => p.PropertyId == updatedProperty.PropertyId && p.OwnerId == user.Id);

                if (property == null) return NotFound();

                // هندلة الموقع
                var location = await _context.Locations.FirstOrDefaultAsync(l => l.City == LocationName);
                if (location == null && !string.IsNullOrEmpty(LocationName))
                {
                    location = new Location { City = LocationName };
                    _context.Locations.Add(location);
                    await _context.SaveChangesAsync();
                }

                // هندلة القسم
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == CategoryName);
                if (category == null && !string.IsNullOrEmpty(CategoryName))
                {
                    category = new Category { Name = CategoryName };
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }

                // تحديث البيانات
                property.Title = updatedProperty.Title;
                property.Description = updatedProperty.Description;
                property.Price = updatedProperty.Price;
                property.Rooms = updatedProperty.Rooms;
                property.Bathrooms = updatedProperty.Bathrooms;
                property.Status = updatedProperty.Status;
                property.LocationId = location?.LocationId ?? updatedProperty.LocationId;
                property.CategoryId = category?.CategoryId ?? updatedProperty.CategoryId;
                property.Latitude = updatedProperty.Latitude;
                property.Longitude = updatedProperty.Longitude;

                // هندلة رفع الصور الجديدة
                var oldPhotos = property.Photos.ToList();
                foreach (var oldPhoto in oldPhotos)
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, oldPhoto.PhotoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);

                    _context.Photos.Remove(oldPhoto);
                }

                // ✅ احفظ المسح الأول
                await _context.SaveChangesAsync();

                // بعدين ضيف الصور الجديدة
                if (imageFiles != null && imageFiles.Count > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    // امسح الصورة الـ Main القديمة بس
                    var mainPhoto = property.Photos.FirstOrDefault(p => p.IsMain);
                    if (mainPhoto != null)
                    {
                        string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, mainPhoto.PhotoUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);

                        _context.Photos.Remove(mainPhoto);
                        await _context.SaveChangesAsync();
                    }

                    // ضيف الصورة الجديدة كـ Main
                    foreach (var file in imageFiles)
                    {
                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        _context.Photos.Add(new Photo
                        {
                            PhotoUrl = "/uploads/" + uniqueFileName,
                            PropertyId = property.PropertyId,
                            IsMain = true
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("MyProperties");
            }

            // الكود السحري هنا: لازم تملا الـ ViewBag تاني لو حصل أي غلط في الفورم ورجعنا لنفس الصفحة
            ViewBag.Locations = new SelectList(_context.Locations, "LocationId", "City");
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");

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
                .Where(p => p.OwnerId == user.Id && p.IsActive)
                .ToListAsync();

            return View(properties);
        }
    }
}
