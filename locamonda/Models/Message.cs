using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 1)]
        [Column(TypeName = "nvarchar(2000)")]
        public string Content { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        [DataType(DataType.DateTime)]
        public DateTime SentAt { get; set; } = DateTime.Now;

        /* Foreign Keys */

        [Required]
        public int SenderId { get; set; }

        [Required]
        public int ReceiverId { get; set; }

        [Required]
        public int PropertyId { get; set; }

        /* Navigation Properties */

        [ForeignKey("SenderId")]
        public Users Sender { get; set; } = null!;

        [ForeignKey("ReceiverId")]
        public Users Receiver { get; set; } = null!;

        [ForeignKey("PropertyId")]
        public Property Property { get; set; } = null!;
    }
}
