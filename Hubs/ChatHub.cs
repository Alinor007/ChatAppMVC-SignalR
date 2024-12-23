using ChatITN100.Data;
using ChatITN100.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ChatITN100.Hubs
{
    public class ChatHub:Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }



        public override async Task OnConnectedAsync()
        {
            var user = Context.User.Identity.Name;
            await Clients.All.SendAsync("UpdateUserStatus", user, "Online");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var user = Context.User.Identity.Name;
            await Clients.All.SendAsync("UpdateUserStatus", user, "Offline");
            await base.OnDisconnectedAsync(exception);
        }
        public async Task JoinRoom(string roomId)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (string.IsNullOrWhiteSpace(roomId))
            {
                await Clients.Caller.SendAsync("Error", "Room ID is required.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("ReceiveMessage", "System", $"{user.name} has joined the room.");
        }

        public async Task LeaveRoom(string roomId)
        {
            var user = await _userManager.GetUserAsync(Context.User);
            if (string.IsNullOrWhiteSpace(roomId))
            {
                await Clients.Caller.SendAsync("Error", "Room ID is required.");
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("ReceiveMessage", "System", $"{user.name} has left the room.");
        }

     
    }
}
