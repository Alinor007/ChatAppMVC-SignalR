using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChatITN100.Data;
using ChatITN100.Models;
using Microsoft.AspNetCore.Identity;

namespace ChatITN100.Controllers
{
    public class RoomsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoomsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // GET: Rooms
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var rooms = await _context.UserRooms
                .Where(ur => ur.UserId == user.Id)
                .Include(ur => ur.Room)
                .Select(ur => ur.Room)
                .ToListAsync();

            return View(rooms);
        }
        ////public async Task<IActionResult> UserList()
        ////{
        ////    var currentUser = await _userManager.GetUserAsync(User);

        ////    if (currentUser == null)
        ////    {
        ////        return Unauthorized();
        ////    }

        ////    // Fetch all users except the logged-in user
        ////    var users = await _context.Users
        ////        .Where(u => u.Id != currentUser.Id)
        ////        .ToListAsync();

        ////    return View(users);
        ////}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> PrivateRoom(string otherUserId)
        //{
        //    var currentUser = await _userManager.GetUserAsync(User);

        //    if (currentUser == null)
        //    {
        //        return Unauthorized();
        //    }

        //    if (string.IsNullOrWhiteSpace(otherUserId))
        //    {
        //        return BadRequest("Invalid user selection.");
        //    }

        //    // Fetch the other user's information
        //    var otherUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == otherUserId);
        //    if (otherUser == null)
        //    {
        //        return NotFound("The selected user does not exist.");
        //    }

        //    // Check if a private room already exists between the two users
        //    var privateRoom = await _context.Rooms
        //        .Include(r => r.UserRooms)
        //        .FirstOrDefaultAsync(r => r.Private &&
        //                                  r.UserRooms.Any(ur => ur.UserId == currentUser.Id) &&
        //                                  r.UserRooms.Any(ur => ur.UserId == otherUserId));

        //    if (privateRoom == null)
        //    {
        //        // Create a new private room with a name based on the users' names
        //        privateRoom = new Room
        //        {
        //            Id = Guid.NewGuid().ToString(),
        //            Name = $"{currentUser.name} and {otherUser.name}", // Room name based on usernames
        //            Private = true
        //        };

        //        _context.Rooms.Add(privateRoom);

        //        // Add both users to the room
        //        _context.UserRooms.AddRange(
        //            new UserRooms { RoomId = privateRoom.Id, UserId = currentUser.Id },
        //            new UserRooms { RoomId = privateRoom.Id, UserId = otherUserId }
        //        );

        //        await _context.SaveChangesAsync();
        //    }

        //    // Redirect to the specific room
        //    return RedirectToAction("Room", "Message", new { roomId = privateRoom.Id });
        //}
        // GET: Rooms/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: Rooms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Rooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Icon,Private")] Room model)
        {
            //if (ModelState.IsValid)
            //{
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Create a new room
            var newRoom = new Room
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                Icon = model.Icon,
                Private = model.Private,
            };

            _context.Rooms.Add(newRoom);
            await _context.SaveChangesAsync();

            // Add the current user to the new room
            var userRoom = new UserRooms
            {
                RoomId = newRoom.Id,
                UserId = user.Id
            };

            _context.UserRooms.Add(userRoom);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "userRooms");

        }

        // GET: Rooms/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        // POST: Rooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Icon,Private")] Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(room);
        }

        // GET: Rooms/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // POST: Rooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room != null)
            {
                _context.Rooms.Remove(room);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RoomExists(string id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
}
