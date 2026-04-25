using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class RealEstate
    {
        public enum RealEstateType
        {
            Apartment,
            House,
            Office,
            Land,
            Villa,
            Shop
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50), MinLength(5)]
        [Display(Name = "Real Estate Title")]
        public string Title { get; set; }

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Location")]
        public string Location { get; set; }

        [Required]
        [Display(Name = "Real Estate Type")]
        public RealEstateType PropertyType { get; set; }

        [Display(Name = "Rooms")]
        public int Rooms { get; set; }

        [Range(0, 50)]
        [Display(Name = "Bathrooms")]
        public int Bathrooms { get; set; }

        [Display(Name = "Image")]
        public string Image { get; set; }

        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = false;

        public bool IsActive { get; set; } = true;


        [ForeignKey("User")]
        public int UserId { get; set; }

        public Users User { get; set; }
        /* Navigation Properties */
        
        /*
    
         Favorites
         Bookings 
         Orders

        */
        public List<Favorite> Favorites { get; set; }

        public List<Booking> Bookings { get; set; }
    }
}