using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Users
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Column(TypeName = "nvarchar(150)")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 20 characters")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Column(TypeName = "nvarchar(20)")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [DataType(DataType.Password)]
        [Column(TypeName = "nvarchar(255)")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Account type is required")]
        [StringLength(20)]
        [RegularExpression("Customer|Owner", ErrorMessage = "Account type must be Customer or Owner")]
        [Column(TypeName = "nvarchar(20)")]
        public string AccountType { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters")]
        [Column(TypeName = "nvarchar(250)")]
        public string Address { get; set; }

        [Range(18, 150, ErrorMessage = "Age must be between 18 and 150")]
        public int Age { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;


        /* Navigation Properties */

        public virtual ICollection<Property> Properties { get; set; }
        public virtual ICollection<Favorite> Favorites { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Message> SentMessages { get; set; }
        public virtual ICollection<Message> ReceivedMessages { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
