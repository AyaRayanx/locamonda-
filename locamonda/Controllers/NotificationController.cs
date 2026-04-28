using locamonda.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace locamonda.Controllers
{
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificationController(ApplicationDbContext context)
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

            var notifications = _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return View(notifications);
        }

        // ─── Mark One as Read ─────────────────

        public IActionResult MarkRead(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            var notification = _context.Notifications
                .FirstOrDefault(n => n.NotificationId == id && n.UserId == userId);

            if (notification == null) return NotFound();

            notification.IsRead = true;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ─── Mark All as Read ─────────────────

        public IActionResult MarkAllRead()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            var unread = _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToList();

            foreach (var n in unread)
                n.IsRead = true;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // ─── Delete ───────────────────────────

        public IActionResult Delete(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            var notification = _context.Notifications
                .FirstOrDefault(n => n.NotificationId == id && n.UserId == userId);

            if (notification == null) return NotFound();

            _context.Notifications.Remove(notification);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}