using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Users : IdentityUser<int>
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters")]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account type is required")]
        [StringLength(20)]
        [RegularExpression("Customer|Owner", ErrorMessage = "Account type must be Customer or Owner")]
        [Column(TypeName = "nvarchar(20)")]
        public string AccountType { get; set; } = string.Empty;

        [Required]
        public bool IsActive { get; set; } = true;

        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters")]
        [Column(TypeName = "nvarchar(250)")]
        public string Address { get; set; } = string.Empty;

        [Range(18, 150, ErrorMessage = "Age must be between 18 and 150")]
        public int Age { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /* Navigation Properties */
        public virtual ICollection<Property> Properties { get; set; } = new List<Property>();
        public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
