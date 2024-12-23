using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ChatITN100.Data;
using ChatITN100.Models;

namespace ChatITN100.Controllers
{
    public class UserRoomsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserRoomsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserRooms
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.UserRooms.Include(u => u.Room).Include(u => u.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: UserRooms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userRooms = await _context.UserRooms
                .Include(u => u.Room)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userRooms == null)
            {
                return NotFound();
            }

            return View(userRooms);
        }

        // GET: UserRooms/Create
        public IActionResult Create()
        {
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id");
            return View();
        }

        // POST: UserRooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RoomId,UserId")] UserRooms userRooms)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userRooms);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", userRooms.RoomId);
            ViewData["UserId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", userRooms.UserId);
            return View(userRooms);
        }

        // GET: UserRooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userRooms = await _context.UserRooms.FindAsync(id);
            if (userRooms == null)
            {
                return NotFound();
            }
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", userRooms.RoomId);
            ViewData["UserId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", userRooms.UserId);
            return View(userRooms);
        }

        // POST: UserRooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomId,UserId")] UserRooms userRooms)
        {
            if (id != userRooms.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userRooms);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserRoomsExists(userRooms.Id))
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
            ViewData["RoomId"] = new SelectList(_context.Rooms, "Id", "Id", userRooms.RoomId);
            ViewData["UserId"] = new SelectList(_context.Set<ApplicationUser>(), "Id", "Id", userRooms.UserId);
            return View(userRooms);
        }

        // GET: UserRooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userRooms = await _context.UserRooms
                .Include(u => u.Room)
                .Include(u => u.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userRooms == null)
            {
                return NotFound();
            }

            return View(userRooms);
        }

        // POST: UserRooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userRooms = await _context.UserRooms.FindAsync(id);
            if (userRooms != null)
            {
                _context.UserRooms.Remove(userRooms);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserRoomsExists(int id)
        {
            return _context.UserRooms.Any(e => e.Id == id);
        }
    }
}
