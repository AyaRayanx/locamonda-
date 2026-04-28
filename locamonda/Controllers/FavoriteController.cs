using locamonda.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavoriteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── Index ─────────────────────────────

        public IActionResult Index()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            var favorites = _context.Favorites
                .Include(f => f.Property)
                    .ThenInclude(p => p.Location)
                .Include(f => f.Property)
                    .ThenInclude(p => p.Photos)
                .Where(f => f.UserId == userId)
                .ToList();

            return View(favorites);
        }

        // ─── Add to Favorites ─────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int propertyId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            bool exists = _context.Favorites
                .Any(f => f.UserId == userId && f.PropertyId == propertyId);

            if (!exists)
            {
                _context.Favorites.Add(new Favorite
                {
                    UserId = userId,
                    PropertyId = propertyId,
                    SavedAt = DateTime.Now
                });

                _context.SaveChanges();
            }

            return RedirectToAction("Details", "Property", new { id = propertyId });
        }

        // ─── Remove ────────────────────────────

        public IActionResult Remove(int favoriteId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            var favorite = _context.Favorites
                .FirstOrDefault(f => f.FavoriteId == favoriteId && f.UserId == userId);

            if (favorite == null) return NotFound();

            _context.Favorites.Remove(favorite);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}