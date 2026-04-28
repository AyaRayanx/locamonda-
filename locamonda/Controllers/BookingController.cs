using locamonda.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
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

            var bookings = _context.Bookings
                .Include(b => b.Property)
                    .ThenInclude(p => p.Location)
                .Include(b => b.Property)
                    .ThenInclude(p => p.Photos)
                .Where(b => b.UserId == userId)
                .ToList();

            return View(bookings);
        }

        // ─── Create GET ────────────────────────

        public IActionResult Create(int propertyId)
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login", "Users");

            var property = _context.Properties.Find(propertyId);
            if (property == null) return NotFound();

            ViewBag.Property = property;
            return View();
        }

        // ─── Create POST ───────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Booking booking)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Users");

            if (ModelState.IsValid)
            {
                if (booking.EndDate <= booking.StartDate)
                {
                    ViewBag.Error = "End date must be after start date";
                    ViewBag.Property = _context.Properties.Find(booking.PropertyId);
                    return View(booking);
                }

                booking.UserId = int.Parse(userIdStr);
                booking.Status = "Pending";
                booking.CreatedAt = DateTime.Now;

                _context.Bookings.Add(booking);

                var property = _context.Properties
                    .FirstOrDefault(p => p.PropertyId == booking.PropertyId);

                if (property != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = property.OwnerId,
                        Type = "NewBooking",
                        NotificationMessage =
                            $"You have a new booking request for: {property.Title}",
                        CreatedAt = DateTime.Now
                    });
                }

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Property = _context.Properties.Find(booking.PropertyId);
            return View(booking);
        }

        // ─── Cancel ────────────────────────────

        public IActionResult Cancel(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            var booking = _context.Bookings
                .FirstOrDefault(b => b.BookingId == id && b.UserId == userId);

            if (booking == null) return NotFound();

            booking.Status = "Cancelled";
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ─── Confirm (Owner) ───────────────────

        public IActionResult Confirm(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            var booking = _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefault(b =>
                    b.BookingId == id &&
                    b.Property.OwnerId == userId);

            if (booking == null) return NotFound();

            booking.Status = "Confirmed";

            _context.Notifications.Add(new Notification
            {
                UserId = booking.UserId,
                Type = "BookingConfirmed",
                NotificationMessage =
                    $"Your booking for {booking.Property.Title} is confirmed!",
                CreatedAt = DateTime.Now
            });

            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // ─── Owner Bookings ────────────────────

        public IActionResult OwnerBookings()
        {
            var type = HttpContext.Session.GetString("AccountType");
            if (type != "Owner")
                return RedirectToAction("Login", "Users");

            var userIdStr = HttpContext.Session.GetString("UserId");
            int userId = int.Parse(userIdStr);

            var bookings = _context.Bookings
                .Include(b => b.Property)
                .Include(b => b.User)
                .Where(b => b.Property.OwnerId == userId)
                .ToList();

            return View(bookings);
        }
    }
}