using locamonda.Models;
using Microsoft.AspNetCore.Mvc;

namespace locamonda.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly AppDbContext _context;

        public FavoriteController(AppDbContext context)
        {
            _context = context;
        }

        // Add 
        public IActionResult Add(int realEstateId, int userId)
        {
            var fav = new Favorite
            {
                RealEstateId = realEstateId,
                UserId = userId
            };

            _context.Favorites.Add(fav);
            _context.SaveChanges();

            return RedirectToAction("Index", "RealEstate");
        }

        //View 
        public IActionResult Index(int userId)
        {
            var favorites = _context.Favorites
                .Where(f => f.UserId == userId)
                .ToList();

            return View(favorites);
        }

        // Remove 
        public IActionResult Remove(int id)
        {
            var fav = _context.Favorites.Find(id);

            if (fav != null)
            {
                _context.Favorites.Remove(fav);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}