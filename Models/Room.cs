using System.ComponentModel.DataAnnotations;

namespace ChatITN100.Models
{
    public class Room
    {
            [Key]
            public string Id { get; set; }  // Primary Key

            [Required]
            public string Name { get; set; } = string.Empty;

            public string Icon { get; set; } = string.Empty;

            public bool Private { get; set; } = false;

            public ICollection<UserRooms> UserRooms { get; set; }

            public ICollection<Message> Messages { get; set; }
    }
}
