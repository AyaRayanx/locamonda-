using locamonda.Data;
using locamonda.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace locamonda.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public ReportController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Notify Admins
        private async Task NotifyAdmins(string type, string message)
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = admin.Id,
                    Type = type,
                    NotificationMessage = message,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
            }
            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public IActionResult Create(int? propertyId, int? userId)
        {
            if (propertyId == null && userId == null) return BadRequest();

            var report = new Report
            {
                PropertyId = propertyId,
                ReportedUserId = userId
            };

            return View(report);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Report report)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            report.ReporterId = user.Id;
            report.CreatedAt = DateTime.Now;
            report.Status = "Pending";

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            // Notify Admins
            await NotifyAdmins("NewReport", $"A new report has been submitted by {user.UserName}.");

            TempData["SuccessMessage"] = "Your report has been submitted for review.";
            
            if (report.PropertyId.HasValue)
                return RedirectToAction("Details", "Property", new { id = report.PropertyId });
            
            return RedirectToAction("Index", "Home");
        }
    }
}
