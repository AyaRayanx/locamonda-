using System.ComponentModel.DataAnnotations;

namespace locamonda.Models
{
    public class RealEstate
    {

        public enum RealEstatetype
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
        [Display(Name = "RealEstate Title")]
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
        [Display(Name = "RealEstateType")]
        public RealEstatetype RealEstateType { get; set; }


        [Display(Name = "Rooms")]
        public int Rooms { get; set; }

        [Range(0, 50)]
        [Display(Name = "Bathrooms")]
        public int Bathrooms { get; set; }


        [Display(Name = "Image")]
        public string Image { get; set; }


        [Display(Name = "DateAdded")]
        public DateTime DateAdded { get; set; }

        public bool IsApproved { get; set; } = false;

        public bool IsActive { get; set; } = true;



        /* Navigation Properties */

        /*
    
         Favorites
         Bookings 
         Orders

        */

    }
}
