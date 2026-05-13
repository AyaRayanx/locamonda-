using locamonda.Data;
using locamonda.Models;
using locamonda.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace locamonda.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public UsersController(
            UserManager<Users> userManager,
            SignInManager<Users> signInManager,
            AppDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // User Register

        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Users
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Name = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    AccountType = model.AccountType,
                    Address = model.Address,
                    Age = model.Age,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign Role
                    await _userManager.AddToRoleAsync(user, model.AccountType);
                    
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Profile", "Users");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // User Login

        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Property");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Your account has been deactivated. Please contact support.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, 
                    model.Password, 
                    model.RememberMe, 
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Property");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // User Logout

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // User Profile
        
        public async Task<IActionResult> Details(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Fetch some stats if needed (e.g. properties owned if it's an owner)
            ViewBag.PropertiesCount = await _context.Properties.CountAsync(p => p.OwnerId == id && p.IsApproved);
            
            return View(user);
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");

            ViewData["HideBooking"] = true;

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(Users updatedUser, IFormFile? profileImage)
        {
            ModelState.Clear();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login");
            
            // Validate basic fields manually if needed, but for now we trust the inputs
            if (string.IsNullOrEmpty(updatedUser.Name))
            {
                ModelState.AddModelError("Name", "Name is required");
                ViewData["HideBooking"] = true;
                return View(user);
            }
            // Save Picture
            if (profileImage != null && profileImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + profileImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(fileStream);
                }
                user.ProfilePicture = "/uploads/" + uniqueFileName;
            }
                // Only update allowed fields
                user.Name = updatedUser.Name;
                user.PhoneNumber = updatedUser.PhoneNumber;
                user.Address = updatedUser.Address;
                user.Age = updatedUser.Age;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    ViewBag.Message = "Profile updated successfully";
                    return View(user);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            // Hide UI
            ViewData["HideBooking"] = true;
            return View(user);
        }
    }
}
