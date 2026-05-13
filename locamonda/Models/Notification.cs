using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [StringLength(100)]
        [RegularExpression("BookingConfirmed|BookingCancelled|NewReview|NewBooking|AccountActivated|AccountDeactivated|PropertyApproved|PropertyRejected|NewProperty|NewReport")]
        [Column(TypeName = "nvarchar(100)")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [StringLength(500, MinimumLength = 5)]
        [Column(TypeName = "nvarchar(500)")]
        public string NotificationMessage { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Foreign Keys
        [Required]
        public int UserId { get; set; }

        // Navigation Properties

        [ForeignKey("UserId")]
        public Users User { get; set; } = null!;
    }
}
