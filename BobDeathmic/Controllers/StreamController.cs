using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Data;
using BobDeathmic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BobDeathmic.Controllers
{
    public class StreamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StreamController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Status));
        }
        public async Task<IActionResult> Status()
        {
            return View(await _context.StreamModels.ToListAsync());
        }
        public async Task<IActionResult> Verwaltung()
        {
            return View(await _context.StreamModels.ToListAsync());
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stream = await _context.StreamModels.FindAsync(id);
            if (stream == null)
            {
                return NotFound();
            }
            ViewData["StreamTypes"] = stream.EnumStreamTypes();
            var relaychannels = await _context.RelayChanels.ToListAsync();
            ViewData["RelayChannels"] = relaychannels;
            ViewData["SelectedRelayChannel"] = stream.DiscordRelayChannel;
            return View(stream);
        }

        // POST: Streams2/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,StreamName,Game,UserID,Url,Type,Started,Stopped,RelayState,StreamState,DiscordRelayChannel")] Stream stream)
        {
            if (id != stream.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(stream);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StreamExists(stream.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Verwaltung));
            }
            return View(stream);
        }

        // GET: Streams2/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stream = await _context.StreamModels
                .FirstOrDefaultAsync(m => m.ID == id);
            if (stream == null)
            {
                return NotFound();
            }

            return View(stream);
        }

        // POST: Streams2/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stream = await _context.StreamModels.FindAsync(id);
            _context.StreamModels.Remove(stream);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Verwaltung));
        }

        private bool StreamExists(int id)
        {
            return _context.StreamModels.Any(e => e.ID == id);
        }
    }
}