using locamonda.Models;
using locamonda.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    [Authorize(Roles = "Owner")]
    public class PhotoController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<Users> _userManager;

        public PhotoController(AppDbContext context, IWebHostEnvironment environment, UserManager<Users> userManager)
        {
            _context = context;
            _environment = environment;
            _userManager = userManager;
        }

        // ─── Upload GET ───────────────────────

        public IActionResult Upload(int propertyId)
        {
            ViewBag.PropertyId = propertyId;
            return View();
        }

        // ─── Upload POST ──────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int propertyId, IFormFile photoFile, bool isMain)
        {
            if (photoFile == null || photoFile.Length == 0)
            {
                ViewBag.Error = "Please select a photo to upload";
                ViewBag.PropertyId = propertyId;
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Verify property ownership
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId && p.OwnerId == user.Id);
            
            if (property == null) return Unauthorized();

            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            // safe file name
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(photoFile.FileName);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photoFile.CopyToAsync(stream);
            }

            // remove old main if needed
            if (isMain)
            {
                var oldMain = await _context.Photos
                    .FirstOrDefaultAsync(p => p.PropertyId == propertyId && p.IsMain);

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
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Property", new { id = propertyId });
        }

        // ─── Delete ───────────────────────────

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var photo = await _context.Photos
                .Include(p => p.Property)
                .FirstOrDefaultAsync(p => p.PhotoId == id && p.Property.OwnerId == user.Id);

            if (photo == null) return NotFound();

            int propertyId = photo.PropertyId;

            string filePath = Path.Combine(_environment.WebRootPath, photo.PhotoUrl.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Property", new { id = propertyId });
        }
    }
}
