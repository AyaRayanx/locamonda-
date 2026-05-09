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

        // ───────── NOTIFICATION HELPER ─────────
        private async Task AddNotification(int userId, string type, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                NotificationMessage = message,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        // ───────── AUTO CANCEL ─────────
        private async Task AutoCancelExpiredBookings()
        {
            var expired = await _context.Bookings
                .Include(b => b.Property)
                .Where(b =>
                    b.Status == "Confirmed" &&
                    b.IsDone == false &&
                    b.ConfirmedAt != null &&
                    b.ConfirmedAt <= DateTime.Now.AddDays(-3))
                .ToListAsync();

            foreach (var b in expired)
            {
                b.Status = "Cancelled";

                // notify USER
                await AddNotification(
                    b.UserId,
                    "BookingCancelled",
                    "Your booking was automatically cancelled due to inactivity"
                );

                // notify OWNER
                await AddNotification(
                    b.Property.OwnerId,
                    "BookingCancelled",
                    "A confirmed booking was automatically cancelled"
                );
            }

            await _context.SaveChangesAsync();
        }

        // ───────── USER BOOKINGS ─────────
        public async Task<IActionResult> Index()
        {
            await AutoCancelExpiredBookings();

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

        // ───────── CREATE GET ─────────
        public async Task<IActionResult> Create(int propertyId)
        {
            var property = await _context.Properties.FindAsync(propertyId);
            if (property == null) return NotFound();

            ViewBag.Property = property;
            return View();
        }

        // ───────── CREATE POST ─────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Property = await _context.Properties.FindAsync(booking.PropertyId);
                return View(booking);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (booking.EndDate <= booking.StartDate)
            {
                ViewBag.Error = "Invalid dates";
                return View(booking);
            }

            var property = await _context.Properties.FindAsync(booking.PropertyId);

            booking.UserId = user.Id;
            booking.Status = "Pending";
            booking.CreatedAt = DateTime.Now;
            booking.IsDone = false;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            if (property == null) return NotFound();

            // NOTIFY OWNER
            await AddNotification(
                property.OwnerId,
                "NewBooking",
                $"New booking request for your property '{property.Title}'"
            );

            return RedirectToAction(nameof(Index));
        }

        // ───────── USER CANCEL ─────────
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.UserId == user.Id);

            if (booking == null) return NotFound();

            booking.Status = "Cancelled";

            await _context.SaveChangesAsync();

            // NOTIFY OWNER
            await AddNotification(
                booking.Property.OwnerId,
                "BookingCancelled",
                "A user cancelled a booking request"
            );

            return RedirectToAction(nameof(Index));
        }

        // ───────── OWNER CONFIRM ─────────
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Confirm(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b =>
                    b.BookingId == id &&
                    b.Property.OwnerId == user.Id);

            if (booking == null) return NotFound();

            booking.Status = "Confirmed";
            booking.ConfirmedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // NOTIFY USER
            await AddNotification(
                booking.UserId,
                "BookingConfirmed",
                "Your booking has been confirmed"
            );

            return RedirectToAction(nameof(OwnerBookings));
        }

        // ───────── OWNER REJECT ─────────
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Reject(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b =>
                    b.BookingId == id &&
                    b.Property.OwnerId == user.Id);

            if (booking == null) return NotFound();

            booking.Status = "NotAvailable";

            await _context.SaveChangesAsync();

            // NOTIFY USER
            await AddNotification(
                booking.UserId,
                "BookingCancelled",
                "Your booking request was rejected"
            );

            return RedirectToAction(nameof(OwnerBookings));
        }

        // ───────── OWNER DONE ─────────
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> MarkAsDone(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var booking = await _context.Bookings
                .Include(b => b.Property)
                .FirstOrDefaultAsync(b =>
                    b.BookingId == id &&
                    b.Property.OwnerId == user.Id);

            if (booking == null) return NotFound();

            booking.IsDone = true;

            await _context.SaveChangesAsync();

            // NOTIFY BOTH
            await AddNotification(booking.UserId, "BookingCompleted", "Your booking is completed");
            await AddNotification(booking.Property.OwnerId, "BookingCompleted", "A booking was marked as completed");

            return RedirectToAction(nameof(OwnerBookings));
        }

        // ───────── OWNER VIEW ─────────
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> OwnerBookings()
        {
            await AutoCancelExpiredBookings();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var bookings = await _context.Bookings
                .Include(b => b.Property)
                .ThenInclude(p => p.Photos)
                .Include(b => b.User)
                .Where(b => b.Property.OwnerId == user.Id)
                .ToListAsync();

            return View(bookings);
        }
    }
}