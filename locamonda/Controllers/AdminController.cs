using locamonda.Data;
using locamonda.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public AdminController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Notification Helper
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

        public async Task<IActionResult> Dashboard()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalProperties = await _context.Properties.CountAsync();
            ViewBag.PendingPropertiesCount = await _context.Properties.CountAsync(p => !p.IsApproved);
            ViewBag.ActiveReportsCount = await _context.Reports.CountAsync(r => r.Status == "Pending");

            // Recent Activity
            ViewBag.RecentReports = await _context.Reports
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentPendingProperties = await _context.Properties
                .Include(p => p.Owner)
                .Where(p => !p.IsApproved)
                .OrderByDescending(p => p.DateAdded)
                .Take(5)
                .ToListAsync();

            return View();
        }

        // User Management
        public async Task<IActionResult> Users()
        {
            // Ensure all Admins are active and protected
            var admins = await _context.Users.Where(u => u.AccountType == "Admin" && !u.IsActive).ToListAsync();
            if (admins.Any())
            {
                foreach (var admin in admins)
                {
                    admin.IsActive = true;
                }
                await _context.SaveChangesAsync();
            }

            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Prevent blocking Admins
            if (user.AccountType == "Admin")
            {
                return BadRequest("Administrators cannot be blocked.");
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            await AddNotification(
                user.Id,
                user.IsActive ? "AccountActivated" : "AccountDeactivated",
                user.IsActive ? "Your account has been reactivated by an administrator." : "Your account has been deactivated by an administrator."
            );

            return RedirectToAction(nameof(Users));
        }

        // Property Management
        public async Task<IActionResult> Properties()
        {
            var properties = await _context.Properties
                .Include(p => p.Owner)
                .Include(p => p.Location)
                .Where(p => !p.IsApproved)
                .ToListAsync();
            return View(properties);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();

            property.IsApproved = true;
            await _context.SaveChangesAsync();

            await AddNotification(
                property.OwnerId,
                "PropertyApproved",
                $"Your property '{property.Title}' has been approved by the administrator."
            );

            return RedirectToAction(nameof(Properties));
        }

        [HttpPost]
        public async Task<IActionResult> RejectProperty(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property == null) return NotFound();

            // Notify owner before deletion or just mark as rejected?
            // Usually rejection might mean deletion or just a flag.
            // In this project, it seems we might just delete it or have a Rejected status.
            // Let's assume we notify them first.
            await AddNotification(
                property.OwnerId,
                "PropertyRejected",
                $"Your property '{property.Title}' has been rejected by the administrator."
            );

            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Properties));
        }

        // Report Management
        public async Task<IActionResult> Reports()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.Property)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> ResolveReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            report.Status = "Resolved";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Reports));
        }

        [HttpPost]
        public async Task<IActionResult> DismissReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            report.Status = "Dismissed";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Reports));
        }
        // Add Admin
        public IActionResult AddAdmin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdmin(locamonda.Models.ViewModels.RegisterViewModel model)
        {
            // Force Admin type
            model.AccountType = "Admin";

            if (ModelState.IsValid)
            {
                var user = new Users
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    Address = model.Address,
                    Age = model.Age,
                    AccountType = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    TempData["Success"] = "Admin account created successfully!";
                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }
    }
}
