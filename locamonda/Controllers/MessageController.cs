using locamonda.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    public class MessageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MessageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── Inbox ─────────────────────────────

        public IActionResult Index()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            var messages = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Property)
                .Where(m => m.ReceiverId == userId)
                .OrderByDescending(m => m.SentAt)
                .ToList();

            return View(messages);
        }

        // ─── Send GET ─────────────────────────

        public IActionResult Send(int propertyId)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Users");

            var property = _context.Properties.Find(propertyId);
            if (property == null) return NotFound();

            ViewBag.Property = property;
            return View();
        }

        // ─── Send POST ────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Send(Message message)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Users");

            int senderId = int.Parse(userIdStr);

            if (ModelState.IsValid)
            {
                var property = _context.Properties.Find(message.PropertyId);
                if (property == null) return NotFound();

                message.SenderId = senderId;
                message.ReceiverId = property.OwnerId;
                message.IsRead = false;
                message.SentAt = DateTime.Now;

                _context.Messages.Add(message);

                // Notification
                _context.Notifications.Add(new Notification
                {
                    UserId = property.OwnerId,
                    Type = "NewMessage",
                    NotificationMessage = $"You have a new message about: {property.Title}",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });

                _context.SaveChanges();

                return RedirectToAction("Details", "Property", new { id = message.PropertyId });
            }

            ViewBag.Property = _context.Properties.Find(message.PropertyId);
            return View(message);
        }

        // ─── Mark as Read ─────────────────────

        public IActionResult MarkRead(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            if (userIdStr == null)
                return RedirectToAction("Login", "Users");

            int userId = int.Parse(userIdStr);

            var message = _context.Messages
                .FirstOrDefault(m => m.MessageId == id && m.ReceiverId == userId);

            if (message == null) return NotFound();

            message.IsRead = true;
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

            var message = _context.Messages
                .FirstOrDefault(m =>
                    m.MessageId == id &&
                    (m.SenderId == userId || m.ReceiverId == userId));

            if (message == null) return NotFound();

            _context.Messages.Remove(message);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}