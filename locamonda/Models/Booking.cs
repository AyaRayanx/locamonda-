using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression("Pending|Confirmed|Cancelled|NotAvailable")]
        [Column(TypeName = "nvarchar(20)")]
        public string Status { get; set; } = "Pending";

        [Required]
        [Range(0.01, 99999999.99)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ConfirmedAt { get; set; }

        public bool IsDone { get; set; } = false;

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; } = null!;

        [ForeignKey("PropertyId")]
        public Property Property { get; set; } = null!;
    }
}