using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int ReporterId { get; set; }

        public int? ReportedUserId { get; set; }

        public int? PropertyId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Resolved, Dismissed

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties

        [ForeignKey("ReporterId")]
        public virtual Users Reporter { get; set; } = null!;

        [ForeignKey("ReportedUserId")]
        public virtual Users? ReportedUser { get; set; }

        [ForeignKey("PropertyId")]
        public virtual Property? Property { get; set; }
    }
}
