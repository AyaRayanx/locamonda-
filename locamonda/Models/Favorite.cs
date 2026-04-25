using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int RealEstateId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        //  Navigation
        public Users User { get; set; }

        public RealEstate RealEstate { get; set; }
    }
}