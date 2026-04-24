using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Users
    {

        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3)]
        [Column(TypeName = "nvarchar")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(150)]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(20, MinimumLength = 10)]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression("Customer|Owner")]
        public string AccountType { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;


        [StringLength(250)]
        public string Address { get; set; }

        [Range(18, 150)]
        public int Age { get; set; }


        /* Navigation Properties */

        //public virtual ICollection<UserFavorite> Favorites { get; set; }
        //public virtual ICollection<Booking> Bookings { get; set; 
        //public virtual ICollection<Order> Orders { get; set; }
    }
}
