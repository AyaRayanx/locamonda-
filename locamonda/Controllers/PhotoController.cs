using locamonda.Models;
using Microsoft.AspNetCore.Mvc;

namespace locamonda.Controllers
{
    public class PhotoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PhotoController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // ─── Upload GET ───────────────────────

        public IActionResult Upload(int propertyId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            ViewBag.PropertyId = propertyId;
            return View();
        }

        // ─── Upload POST ──────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(int propertyId, IFormFile photoFile, bool isMain)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            if (photoFile == null || photoFile.Length == 0)
            {
                ViewBag.Error = "Please select a photo to upload";
                ViewBag.PropertyId = propertyId;
                return View();
            }

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            // safe file name
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(photoFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                photoFile.CopyTo(stream);
            }

            // remove old main if needed
            if (isMain)
            {
                var oldMain = _context.Photos
                    .FirstOrDefault(p => p.PropertyId == propertyId && p.IsMain);

                if (oldMain != null)
                    oldMain.IsMain = false;
            }

            var photo = new Photo
            {
                PhotoUrl = "/uploads/" + uniqueFileName,
                IsMain = isMain,
                UploadedAt = DateTime.Now,
                PropertyId = propertyId
            };

            _context.Photos.Add(photo);
            _context.SaveChanges();

            return RedirectToAction("Details", "Property", new { id = propertyId });
        }

        // ─── Delete ───────────────────────────

        public IActionResult Delete(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Users");

            var photo = _context.Photos.Find(id);
            if (photo == null) return NotFound();

            int propertyId = photo.PropertyId;

            string filePath = Path.Combine(_environment.WebRootPath, photo.PhotoUrl.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.Photos.Remove(photo);
            _context.SaveChanges();

            return RedirectToAction("Details", "Property", new { id = propertyId });
        }
    }
}