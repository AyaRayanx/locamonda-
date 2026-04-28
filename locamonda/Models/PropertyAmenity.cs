using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class PropertyAmenity
    {
        [Key]
        public int PropertyAmenityId { get; set; }


        /* Foreign Keys */

        [Required(ErrorMessage = "Property is required")]
        public int PropertyId { get; set; }

        [Required(ErrorMessage = "Amenity is required")]
        public int AmenityId { get; set; }


        /* Navigation Properties */

        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; }

        [ForeignKey("AmenityId")]
        public virtual Amenity Amenity { get; set; }
    }
}
