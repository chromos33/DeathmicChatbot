using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BobDeathmic.Data;
using BobDeathmic.Models;

namespace BobDeathmic.Controllers
{
    public class StreamCommandsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StreamCommandsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: StreamCommands
        public async Task<IActionResult> Index()
        {
            return View(await _context.StreamCommand.Include(sc => sc.stream).ToListAsync());
        }

        // GET: StreamCommands/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var streamCommand = await _context.StreamCommand.Include(sc => sc.stream)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (streamCommand == null)
            {
                return NotFound();
            }

            return View(streamCommand);
        }

        // GET: StreamCommands/Create
        public IActionResult Create()
        {
            ViewData["Streams"] = _context.StreamModels.ToList();
            ViewData["Modes"] = new List<StreamCommandMode> { StreamCommandMode.Manual,StreamCommandMode.Auto };
            return View();
        }

        // POST: StreamCommands/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,name,response,Mode,AutoInverval,streamID")] StreamCommand streamCommand)
        {
            if (ModelState.IsValid)
            {
                _context.Add(streamCommand);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(streamCommand);
        }

        // GET: StreamCommands/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var streamCommand = await _context.StreamCommand.FindAsync(id);
            if (streamCommand == null)
            {
                return NotFound();
            }
            streamCommand.SetSelectableStreams(_context.StreamModels.Select(a => new SelectListItem()
            {
                Value = a.ID.ToString(),
                Text = a.StreamName
            }).ToList());
            ViewData["Streams"] = _context.StreamModels.ToList();
            ViewData["Modes"] = new List<StreamCommandMode> { StreamCommandMode.Manual, StreamCommandMode.Auto, StreamCommandMode.Random };
            return View(streamCommand);
        }

        // POST: StreamCommands/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,name,response,Mode,AutoInverval,streamID")] StreamCommand streamCommand)
        {
            if (id != streamCommand.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(streamCommand);
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StreamCommandExists(streamCommand.ID))
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
            return View(streamCommand);
        }

        // GET: StreamCommands/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var streamCommand = await _context.StreamCommand.Include(sc => sc.stream)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (streamCommand == null)
            {
                return NotFound();
            }

            return View(streamCommand);
        }

        // POST: StreamCommands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var streamCommand = await _context.StreamCommand.FindAsync(id);
            _context.StreamCommand.Remove(streamCommand);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StreamCommandExists(int id)
        {
            return _context.StreamCommand.Any(e => e.ID == id);
        }
    }
}
