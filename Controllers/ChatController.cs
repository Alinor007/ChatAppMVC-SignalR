using ChatITN100.Data;
using ChatITN100.Hubs;
using ChatITN100.Models;
using ChatITN100.Models.viewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatITN100.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<ChatHub> _chatHub;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<ChatHub> chatHub)
        {
            _context = context;
            _userManager = userManager;
            _chatHub = chatHub;
        }


          public async Task<IActionResult> UserList()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Fetch all users except the logged-in user
            var users = await _context.Users
                .Where(u => u.Id != currentUser.Id)
                .ToListAsync();

            return View(users);
        }

        public async Task<IActionResult> Index()
        {

            var userName = User.Identity.Name;
            var rooms = await _context.Rooms
                .Include(r => r.UserRooms)
                .ThenInclude(ur => ur.User)
                .ToListAsync();

            var model = new ChatRoomViewModel
            {
                LoggedInUserName = userName,
                GetRooms = rooms,
                Messages = new List<Message>() // Initial empty messages
            };

            return View(model);
        }
        [Route("Chat/Room/{id}")]
        public async Task<IActionResult> Room(string id)
        {
            var loggedInUser = await _userManager.GetUserAsync(User);
            if (loggedInUser == null)
            {
                return Unauthorized();
            }

            // Fetch the room details along with its users
            var room = await _context.Rooms
                .Include(r => r.UserRooms)
                .ThenInclude(ur => ur.User)
                .FirstOrDefaultAsync(r => r.Id == id);


            if (room == null) return NotFound();

            // Identify the other user in the room
            var otherUser = room.UserRooms
                .Where(ur => ur.UserId != loggedInUser.Id)
                .Select(ur => ur.User.name)
                .FirstOrDefault();
            // Fetch the messages for the room
            var messages = await _context.Messages
                .Where(m => m.RoomId == id)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
            var userlist = await _context.Users
               .Where(u => u.Id != loggedInUser.Id)
               .ToListAsync();

            var model = new ChatRoomViewModel
            {
                Room = room,
                LoggedInUserName = loggedInUser.name,
                OtherUserName = otherUser ?? "Unknown User",
                Messages = messages,
                user = userlist
            };
            //await _chatHub.Clients.Group(id).SendAsync("ReceiveMessage", model.LoggedInUserName, model.OtherUserName);

            return View(model);
        }




        [HttpPost]
        public async Task<IActionResult> SendMessage(string roomId, string payload, IFormFile? file)
        {
            string fileUrl = string.Empty;
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (file != null)
            {
                var filePath = Path.Combine("wwwroot/uploads", file.FileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
                fileUrl = "/uploads/" + file.FileName;
            }

            var message = new Message
            {
                RoomId = roomId,
                Sender = user.name,
                Payload = payload,
                OrderId = await GetNextOrderId(roomId),
                FileUrl = fileUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            // Fix for SendMessage issue
            // Notify all clients in the room using SignalR
            await _chatHub.Clients.Group(roomId).SendAsync("ReceiveMessage", message.Sender, message.Payload, message.FileUrl, message.CreatedAt);


            return RedirectToAction("Room", new { id = roomId });
        }
        // POST: Create or navigate to a private room
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PrivateRoom(string otherUserId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(otherUserId))
            {
                return BadRequest("Invalid user selection.");
            }

            // Fetch the other user's information
            var otherUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == otherUserId);
            if (otherUser == null)
            {
                return NotFound("The selected user does not exist.");
            }

            // Check if a private room already exists between the two users
            var privateRoom = await _context.Rooms
                .Include(r => r.UserRooms)
                .FirstOrDefaultAsync(r => r.Private &&
                                          r.UserRooms.Any(ur => ur.UserId == currentUser.Id) &&
                                          r.UserRooms.Any(ur => ur.UserId == otherUserId));

            if (privateRoom == null)
            {
                // Create a new private room with a name based on the users' names
                privateRoom = new Room
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"{currentUser.UserName} and {otherUser.UserName}", // Room name based on usernames
                    Private = true
                };

                _context.Rooms.Add(privateRoom);

                // Add both users to the room
                _context.UserRooms.AddRange(
                    new UserRooms { RoomId = privateRoom.Id, UserId = currentUser.Id },
                    new UserRooms { RoomId = privateRoom.Id, UserId = otherUserId }
                );

                await _context.SaveChangesAsync();
            }

            // Redirect to the specific room
            return Redirect($"/Chat/Room/{privateRoom.Id}");
        }
        private async Task<int> GetNextOrderId(string roomId)
        {
            try
            {
                var lastMessage = await _context.Messages
                    .Where(m => m.RoomId == roomId)
                    .OrderByDescending(m => m.OrderId)
                    .FirstOrDefaultAsync();

                return lastMessage != null ? lastMessage.OrderId + 1 : 1;
            }
            catch (Exception ex)
            {
                // Log any error
                Console.WriteLine($"Error getting the next order ID: {ex.Message}");
                return 1; // Default to 1 if there's an error
            }
        }
    }
}
