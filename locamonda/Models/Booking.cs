using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int RealEstateId { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Pending";

        // Navigation
        public Users User { get; set; }

        public RealEstate RealEstate { get; set; }
    }
}