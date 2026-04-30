using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Property
    {
        [Key]
        public int PropertyId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 5)]
        [Column(TypeName = "nvarchar(200)")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(2000, MinimumLength = 20)]
        [Column(TypeName = "nvarchar(2000)")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 99999999.99)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Range(1, 50)]
        public int Rooms { get; set; }

        [Range(1, 20)]
        public int Bathrooms { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression("Available|Sold|Rented")]
        [Column(TypeName = "nvarchar(20)")]
        public string Status { get; set; } = "Available";

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        /* Foreign Keys */

        [Required]
        public int OwnerId { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        /* Navigation Properties */

        [ForeignKey("OwnerId")]
        public Users Owner { get; set; } = null!;

        [ForeignKey("LocationId")]
        public Location Location { get; set; } = null!;

        [ForeignKey("CategoryId")]
        public Category Category { get; set; } = null!;

        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
        public ICollection<PropertyAmenity> PropertyAmenities { get; set; } = new List<PropertyAmenity>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
