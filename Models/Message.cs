using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ChatITN100.Models
{
    public class Message
    {
        [Key] 
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-generate the ID
        public string Id { get; set; }  
        public string Sender { get; set; } =string.Empty;

        public string Payload { get; set; } = string.Empty;

        public int OrderId { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string RoomId { get; set; } = string.Empty;


        [ForeignKey("RoomId")]
        public Room Room { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        [DisplayName("Image Name")]
        public string FileUrl { get; set; } = string.Empty;


        [NotMapped]
        [DisplayName("Upload File")]
        public IFormFile ImageFile { get; set; }

    }

}
