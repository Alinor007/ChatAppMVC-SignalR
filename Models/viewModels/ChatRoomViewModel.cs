namespace ChatITN100.Models.viewModels
{
    public class ChatRoomViewModel
    {
        public Room Room { get; set; }
        public string LoggedInUserName { get; set; }
        public string OtherUserName { get; set; }
        public IEnumerable<Message> Messages { get; set; }
        public IEnumerable<Room> GetRooms { get; set; }
        public IEnumerable< ApplicationUser> user { get; set; }
    }
}
