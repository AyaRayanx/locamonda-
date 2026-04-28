using locamonda.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── Create Review ────────────────────────────────────────

        public IActionResult Create(int propertyId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login", "Users");

            ViewBag.PropertyId = propertyId;
            return View();
        }

        [HttpPost]
        public IActionResult Create(Review review)
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login", "Users");

            if (ModelState.IsValid)
            {
                int userId = int.Parse(HttpContext.Session.GetString("UserId"));

                bool alreadyReviewed = _context.Reviews
                    .Any(r => r.UserId == userId && r.PropertyId == review.PropertyId);

                if (alreadyReviewed)
                {
                    ViewBag.Error = "You have already reviewed this property";
                    ViewBag.PropertyId = review.PropertyId;
                    return View(review);
                }

                review.UserId = userId;
                review.CreatedAt = DateTime.Now;

                _context.Reviews.Add(review);
                _context.SaveChanges();

                return RedirectToAction("Details", "Property", new { id = review.PropertyId });
            }

            ViewBag.PropertyId = review.PropertyId;
            return View(review);
        }

        // ─── Delete Review ────────────────────────────────────────

        public IActionResult Delete(int id)
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login", "Users");

            int userId = int.Parse(HttpContext.Session.GetString("UserId"));

            var review = _context.Reviews
                .FirstOrDefault(r => r.ReviewId == id && r.UserId == userId);

            if (review == null)
                return NotFound();

            int propertyId = review.PropertyId;

            _context.Reviews.Remove(review);
            _context.SaveChanges();

            return RedirectToAction("Details", "Property", new { id = propertyId });
        }
    }
}