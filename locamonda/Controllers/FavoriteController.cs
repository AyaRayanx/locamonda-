using locamonda.Models;
using locamonda.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public FavoriteController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ─── Index ─────────────────────────────
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var favorites = await _context.Favorites
                .Include(f => f.Property)
                    .ThenInclude(p => p.Location)
                .Include(f => f.Property)
                    .ThenInclude(p => p.Photos)
                .Where(f => f.UserId == user.Id)
                .ToListAsync();

            return View(favorites);
        }

        // ─── Add to Favorites ─────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int propertyId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            bool exists = await _context.Favorites
                .AnyAsync(f => f.UserId == user.Id && f.PropertyId == propertyId);

            if (!exists)
            {
                _context.Favorites.Add(new Favorite
                {
                    UserId = user.Id,
                    PropertyId = propertyId,
                    SavedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Property", new { id = propertyId });
        }

        // ─── Remove ────────────────────────────

        public async Task<IActionResult> Remove(int favoriteId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.FavoriteId == favoriteId && f.UserId == user.Id);

            if (favorite == null) return NotFound();

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
