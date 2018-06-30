using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BobDeathmic.Data;
using BobDeathmic.Models;
using Microsoft.AspNetCore.Authorization;

namespace BobDeathmic.Controllers
{
    [Authorize]
    public class ExternalAuthController : Controller
    {
        private readonly BobCoreDBContext _context;

        public ExternalAuthController(BobCoreDBContext context)
        {
            _context = context;
        }

        // GET: ExternalAuth
        public async Task<IActionResult> Index()
        {
            return View(await _context.AuthData.ToListAsync());
        }

        // GET: ExternalAuth/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var externalAuthData = await _context.AuthData
                .SingleOrDefaultAsync(m => m.ID == id);
            if (externalAuthData == null)
            {
                return NotFound();
            }

            return View(externalAuthData);
        }

        // GET: ExternalAuth/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ExternalAuth/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Token,Login,Platform")] ExternalAuthData externalAuthData)
        {
            if (ModelState.IsValid)
            {
                _context.Add(externalAuthData);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(externalAuthData);
        }

        // GET: ExternalAuth/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var externalAuthData = await _context.AuthData.SingleOrDefaultAsync(m => m.ID == id);
            if (externalAuthData == null)
            {
                return NotFound();
            }
            return View(externalAuthData);
        }

        // POST: ExternalAuth/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Token,Login,Platform")] ExternalAuthData externalAuthData)
        {
            if (id != externalAuthData.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(externalAuthData);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExternalAuthDataExists(externalAuthData.ID))
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
            return View(externalAuthData);
        }

        // GET: ExternalAuth/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var externalAuthData = await _context.AuthData
                .SingleOrDefaultAsync(m => m.ID == id);
            if (externalAuthData == null)
            {
                return NotFound();
            }

            return View(externalAuthData);
        }

        // POST: ExternalAuth/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var externalAuthData = await _context.AuthData.SingleOrDefaultAsync(m => m.ID == id);
            _context.AuthData.Remove(externalAuthData);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExternalAuthDataExists(int id)
        {
            return _context.AuthData.Any(e => e.ID == id);
        }
    }
}
