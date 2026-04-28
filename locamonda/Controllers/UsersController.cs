using locamonda.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace locamonda.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── Register ───────────────────────────────────────────

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Users user)
        {
            if (ModelState.IsValid)
            {
                bool emailExists = _context.Users.Any(u => u.Email == user.Email);

                if (emailExists)
                {
                    ViewBag.Error = "Email is already registered";
                    return View(user);
                }

                user.CreatedAt = DateTime.Now;
                user.IsActive = true;

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }

            return View(user);
        }

        // ─── Login ───────────────────────────────────────────────

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users
                .FirstOrDefault(u =>
                    u.Email == email &&
                    u.Password == password &&
                    u.IsActive);

            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserName", user.Name);
                HttpContext.Session.SetString("AccountType", user.AccountType);

                return RedirectToAction("Index", "Property");
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }

        // ─── Logout ──────────────────────────────────────────────

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ─── Profile ─────────────────────────────────────────────

        public IActionResult Profile()
        {
            string userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login");

            int userId = int.Parse(userIdStr);

            var user = _context.Users.Find(userId);

            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        [HttpPost]
        public IActionResult Profile(Users updatedUser)
        {
            string userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                int userId = int.Parse(userIdStr);

                var user = _context.Users.Find(userId);

                if (user == null)
                    return RedirectToAction("Login");

                user.Name = updatedUser.Name;
                user.PhoneNumber = updatedUser.PhoneNumber;
                user.Address = updatedUser.Address;
                user.Age = updatedUser.Age;

                _context.SaveChanges();

                HttpContext.Session.SetString("UserName", user.Name);

                return RedirectToAction("Profile");
            }

            return View(updatedUser);
        }
    }
}