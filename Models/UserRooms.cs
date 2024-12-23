using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatITN100.Models
{
    public class UserRooms
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RoomId { get; set; }  // Foreign Key referencing Room

        [Required]
        public string UserId { get; set; }  // Foreign Key referencing ApplicationUser

        // Navigation Properties
        [ForeignKey("RoomId")]
        public Room Room { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
