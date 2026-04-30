using locamonda.Models;
using locamonda.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public ReviewController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ─── Create Review ────────────────────────────────────────

        public IActionResult Create(int propertyId)
        {
            ViewBag.PropertyId = propertyId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review review)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();

                bool alreadyReviewed = await _context.Reviews
                    .AnyAsync(r => r.UserId == user.Id && r.PropertyId == review.PropertyId);

                if (alreadyReviewed)
                {
                    ViewBag.Error = "You have already reviewed this property";
                    ViewBag.PropertyId = review.PropertyId;
                    return View(review);
                }

                review.UserId = user.Id;
                review.CreatedAt = DateTime.Now;

                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Property", new { id = review.PropertyId });
            }

            ViewBag.PropertyId = review.PropertyId;
            return View(review);
        }

        // ─── Delete Review ────────────────────────────────────────

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == id && r.UserId == user.Id);

            if (review == null)
                return NotFound();

            int propertyId = review.PropertyId;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Property", new { id = propertyId });
        }
    }
}
