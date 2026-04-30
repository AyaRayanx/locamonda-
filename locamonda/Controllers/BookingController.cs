using locamonda.Models;
using locamonda.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public BookingController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ─── Index (My Bookings) ───────────────

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var bookings = await _context.Bookings
                .Include(b => b.Property)
                    .ThenInclude(p => p.Location)
                .Include(b => b.Property)
                    .ThenInclude(p => p.Photos)
                .Where(b => b.UserId == user.Id)
                .ToListAsync();

            return View(bookings);
        }

        // ─── Create GET ────────────────────────

        public async Task<IActionResult> Create(int propertyId)
        {
            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null) return NotFound();

            ViewBag.Property = property;
            return View();
        }

        // ─── Create POST ───────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Challenge();

                if (booking.EndDate <= booking.StartDate)
                {
                    ViewBag.Error = "End date must be after start date";
                    ViewBag.Property = await _context.Properties.FindAsync(booking.PropertyId);
                    return View(booking);
                }

                booking.UserId = user.Id;
                booking.Status = "Pending";
                booking.CreatedAt = DateTime.Now;

                _context.Bookings.Add(booking);

                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PropertyId == booking.PropertyId);

                if (property != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = property.OwnerId,
                        Type = "NewBooking",
                        NotificationMessage = $"You have a new booking request for: {property.Title}",
                        CreatedAt = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Property = await _context.Properties.FindAsync(booking.PropertyId);
            return View(booking);
        }

        // ─── Cancel ────────────────────────────

        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == user.Id);

            if (booking == null) return NotFound();

            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ─── Confirm (Owner) ───────────────────

        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Confirm(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.Property.OwnerId == user.Id);

            if (booking == null) return NotFound();

            booking.Status = "Confirmed";

            _context.Notifications.Add(new Notification
            {
                UserId = booking.UserId,
                Type = "BookingConfirmed",
                NotificationMessage = $"Your booking for {booking.Property.Title} is confirmed!",
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(OwnerBookings));
        }

        // ─── Owner Bookings ────────────────────

        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> OwnerBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var bookings = await _context.Bookings
                .Include(b => b.Property)
                .Include(b => b.User)
                .Where(b => b.Property.OwnerId == user.Id)
                .ToListAsync();

            return View(bookings);
        }
    }
}
