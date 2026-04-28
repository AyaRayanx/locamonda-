using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Photo
    {
        [Key]
        public int PhotoId { get; set; }

        [Required]
        [StringLength(500)]
        [Column(TypeName = "nvarchar(500)")]
        [Url]
        public string PhotoUrl { get; set; } = string.Empty;

        public bool IsMain { get; set; } = false;

        [DataType(DataType.DateTime)]
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        /* Foreign Keys */

        [Required]
        public int PropertyId { get; set; }

        /* Navigation Properties */

        [ForeignKey("PropertyId")]
        public Property Property { get; set; } = null!;
    }
}