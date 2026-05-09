//using locamonda.Models;
//using locamonda.Data;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

//namespace locamonda.Controllers
//{
//    [Authorize]
//    public class MessageController : Controller
//    {
//        private readonly AppDbContext _context;
//        private readonly UserManager<Users> _userManager;

//        public MessageController(AppDbContext context, UserManager<Users> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        // ─── Inbox ─────────────────────────────

//        public async Task<IActionResult> Index()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null) return Challenge();

//            var messages = await _context.Messages
//                .Include(m => m.Sender)
//                .Include(m => m.Property)
//                .Where(m => m.ReceiverId == user.Id)
//                .OrderByDescending(m => m.SentAt)
//                .ToListAsync();

//            return View(messages);
//        }

//        // ─── Send GET ─────────────────────────

//        public async Task<IActionResult> Send(int propertyId)
//        {
//            var property = await _context.Properties.FindAsync(propertyId);
//            if (property == null) return NotFound();

//            ViewBag.Property = property;
//            return View();
//        }

//        // ─── Send POST ────────────────────────

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Send(Message message)
//        {
//            if (ModelState.IsValid)
//            {
//                var user = await _userManager.GetUserAsync(User);
//                if (user == null) return Challenge();

//                var property = await _context.Properties.FindAsync(message.PropertyId);
//                if (property == null) return NotFound();

//                message.SenderId = user.Id;
//                message.ReceiverId = property.OwnerId;
//                message.IsRead = false;
//                message.SentAt = DateTime.Now;

//                _context.Messages.Add(message);

//                // Notification
//                _context.Notifications.Add(new Notification
//                {
//                    UserId = property.OwnerId,
//                    Type = "NewMessage",
//                    NotificationMessage = $"You have a new message about: {property.Title}",
//                    IsRead = false,
//                    CreatedAt = DateTime.Now
//                });

//                await _context.SaveChangesAsync();

//                return RedirectToAction("Details", "Property", new { id = message.PropertyId });
//            }

//            ViewBag.Property = await _context.Properties.FindAsync(message.PropertyId);
//            return View(message);
//        }

//        // ─── Mark as Read ─────────────────────

//        public async Task<IActionResult> MarkRead(int id)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null) return Challenge();

//            var message = await _context.Messages
//                .FirstOrDefaultAsync(m => m.MessageId == id && m.ReceiverId == user.Id);

//            if (message == null) return NotFound();

//            message.IsRead = true;
//            await _context.SaveChangesAsync();

//            return RedirectToAction(nameof(Index));
//        }

//        // ─── Delete ───────────────────────────

//        public async Task<IActionResult> Delete(int id)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null) return Challenge();

//            var message = await _context.Messages
//                .FirstOrDefaultAsync(m =>
//                    m.MessageId == id &&
//                    (m.SenderId == user.Id || m.ReceiverId == user.Id));

//            if (message == null) return NotFound();

//            _context.Messages.Remove(message);
//            await _context.SaveChangesAsync();

//            return RedirectToAction(nameof(Index));
//        }
//    }
//}





using locamonda.Models;
using locamonda.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    [Authorize]
    public class MessageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public MessageController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ─── Inbox ─────────────────────────────

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Property)
                .Where(m => m.ReceiverId == user.Id)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return View(messages);
        }

        // ─── Send GET ─────────────────────────

        public async Task<IActionResult> Send(int propertyId)
        {
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.PropertyId == propertyId);

            if (property == null)
                return NotFound();

            var owner = await _userManager.FindByIdAsync(property.OwnerId.ToString());

            ViewBag.Property = property;
            ViewBag.Owner = owner;

            return View();
        }

        // ─── Send POST ────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(Message message)
        {
            //ModelState.Remove("Sender");
            //ModelState.Remove("Receiver");
            //ModelState.Remove("SenderId");
            //ModelState.Remove("ReceiverId");

            //if (!ModelState.IsValid)
            //    return Json(new { success = false });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false });

            var property = await _context.Properties.FindAsync(message.PropertyId);
            if (property == null) return Json(new { success = false });

            message.SenderId = user.Id;
            message.ReceiverId = property.OwnerId;
            message.IsRead = false;
            message.SentAt = DateTime.Now;

            _context.Messages.Add(message);

            _context.Notifications.Add(new Notification
            {
                UserId = property.OwnerId,
                Type = "NewMessage",
                NotificationMessage = $"You have a new message about: {property.Title}",
                IsRead = false,
                CreatedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        // ─── Mark as Read ─────────────────────

        public async Task<IActionResult> MarkRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.MessageId == id && m.ReceiverId == user.Id);

            if (message == null) return NotFound();

            message.IsRead = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ─── Delete ───────────────────────────

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var message = await _context.Messages
                .FirstOrDefaultAsync(m =>
                    m.MessageId == id &&
                    (m.SenderId == user.Id || m.ReceiverId == user.Id));

            if (message == null) return NotFound();

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}