using BobDeathmic.Data;
using BobDeathmic.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.Controllers
{
    public class DiscordBansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscordBansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DiscordBans
        [Authorize(Roles = "Dev,Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.DiscordBans.AsQueryable().ToListAsync());
        }
        // GET: DiscordBans/Delete/5
        [Authorize(Roles = "Dev,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discordBan = await _context.DiscordBans.AsQueryable()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (discordBan == null)
            {
                return NotFound();
            }

            return View(discordBan);
        }

        // POST: DiscordBans/Delete/5
        [Authorize(Roles = "Dev,Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var discordBan = await _context.DiscordBans.FindAsync(id);
            _context.DiscordBans.Remove(discordBan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DiscordBanExists(int id)
        {
            return _context.DiscordBans.Any(e => e.Id == id);
        }
    }
}
