using locamonda.Models;
using Microsoft.AspNetCore.Mvc;

namespace locamonda.Controllers
{
    public class MessageController : Controller
    {
        private readonly AppDbContext _context;

        public MessageController(AppDbContext context)
        {
            _context = context;
        }

        //Send message
        public IActionResult Send(int senderId, int receiverId, int realEstateId, string text)
        {
            var msg = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                RealEstateId = realEstateId,
                MessageText = text
            };

            _context.Messages.Add(msg);
            _context.SaveChanges();

            return RedirectToAction("Index", "RealEstate");
        }

        // Inbox
        public IActionResult Inbox(int userId)
        {
            var messages = _context.Messages
                .Where(m => m.ReceiverId == userId)
                .ToList();

            return View(messages);
        }
    }
}