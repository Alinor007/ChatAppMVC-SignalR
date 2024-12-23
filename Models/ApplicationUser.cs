using Microsoft.AspNetCore.Identity;

namespace ChatITN100.Models
{
    public class ApplicationUser:IdentityUser
    {
        public ICollection<UserRooms> UserRooms { get; set; }
        public string name { get; set; } = string.Empty;
    }
}
