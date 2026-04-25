using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace locamonda.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public int ReceiverId { get; set; }

        public int RealEstateId { get; set; }

        [Required]
        public string MessageText { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        public Users Sender { get; set; }
        public Users Receiver { get; set; }
        public RealEstate RealEstate { get; set; }
    }
}