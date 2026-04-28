using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Location
    {
        [Key]
        public int LocationId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Column(TypeName = "nvarchar(100)")]
        public string Country { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Column(TypeName = "nvarchar(100)")]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        public string? District { get; set; }

        [StringLength(20)]
        [Column(TypeName = "nvarchar(20)")]
        public string? ZipCode { get; set; }

        /* Navigation Properties */

        public ICollection<Property> Properties { get; set; }
            = new List<Property>();
    }
}