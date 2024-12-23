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
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostEnvironment;

        public MessagesController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
            _userManager = userManager;
        }
        // GET: Messages
        public async Task<IActionResult> Index()
        {
            // Fetch messages and include related room data
            var messages = await _context.Messages.Include(m => m.Room).ToListAsync();

            return View(messages);
        }

        // GET: Messages/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // GET: Messages/Create
        public IActionResult Create()
        {
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id");
            return View();
        }

        // POST: Messages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Sender,Payload,OrderId,RoomId,ImageFile")] Message message)
        {
            //if (ModelState.IsValid)
            //{
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            if (message.ImageFile != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Path.GetFileNameWithoutExtension(message.ImageFile.FileName);
                    string extension = Path.GetExtension(message.ImageFile.FileName);

                    // Append timestamp to the file name to avoid overwriting
                    fileName = fileName + DateTime.Now.ToString("yyMMddHHmmss") + extension;
                    string path = Path.Combine(wwwRootPath, "Image");

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    path = Path.Combine(path, fileName);

                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await message.ImageFile.CopyToAsync(fileStream);
                    }

                // Save relative path to the database
                message.FileUrl = fileName;
                }
            else
            {
                message.FileUrl = "defaultImage"; // Set a default image
            }

            message.Sender = user.UserName;
                message.OrderId = await GetNextOrderId(message.RoomId); 
                _context.Add(message);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            //}

            //ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", message.RoomId);
            //return View(message);
        }

        // GET: Messages/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages.FindAsync(id);
            if (message == null)
            {
                return NotFound();
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", message.RoomId);
            return View(message);
        }

        // POST: Messages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // GET: Messages/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Sender,Payload,OrderId,RoomId,ImageFile,FileUrl")] Message message)
        {
            if (id != message.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (message.ImageFile != null)
                    {
                        // Get the wwwroot path
                        var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                        Directory.CreateDirectory(uploadsFolder);

                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(message.ImageFile.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await message.ImageFile.CopyToAsync(stream);
                        }

                        // Update the file path
                        message.FileUrl = $"/uploads/{fileName}";
                    }

                    _context.Update(message);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MessageExists(message.Id))
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
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", message.RoomId);
            return View(message);
        }



        // GET: Messages/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var message = await _context.Messages
                .Include(m => m.Room)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (message == null)
            {
                return NotFound();
            }

            return View(message);
        }

        // POST: Messages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var message = await _context.Messages.FindAsync(id);
            if (message != null)
            {
                _context.Messages.Remove(message);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MessageExists(string id)
        {
            return _context.Messages.Any(e => e.Id == id);
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
