using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Amenity
    {
        [Key]
        public int AmenityId { get; set; }

        [Required(ErrorMessage = "Amenity name is required")]
        [StringLength(100, MinimumLength = 2)]
        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string? Icon { get; set; }

        /* Navigation Properties */

        public ICollection<PropertyAmenity> PropertyAmenities { get; set; }
            = new List<PropertyAmenity>();
    }
}