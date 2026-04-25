using locamonda.Models;
using Microsoft.AspNetCore.Mvc;

namespace locamonda.Controllers
{
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        // Create
        public IActionResult Book(int realEstateId, int userId)
        {
            var booking = new Booking
            {
                RealEstateId = realEstateId,
                UserId = userId,
                Status = "Pending"
            };

            _context.Bookings.Add(booking);
            _context.SaveChanges();

            return RedirectToAction("Index", "RealEstate");
        }

        // View
        public IActionResult Index()
        {
            var bookings = _context.Bookings.ToList();
            return View(bookings);
        }
    }
}