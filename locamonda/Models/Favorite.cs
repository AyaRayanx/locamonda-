using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime SavedAt { get; set; } = DateTime.Now;

        /* Foreign Keys */

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        /* Navigation Properties */

        [ForeignKey("UserId")]
        public Users User { get; set; } = null!;

        [ForeignKey("PropertyId")]
        public Property Property { get; set; } = null!;
    }
}