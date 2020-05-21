using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.StreamModels;
using BobDeathmic.Models;
using BobDeathmic.ViewModels.ReactDataClasses.Table;
using BobDeathmic.ViewModels.ReactDataClasses.Table.Columns;
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
            //await _context.StreamCommand.Include(sc => sc.stream).ToListAsync()
            return View();
        }

        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<string> Data()
        {
            Table table = new Table();
            Row Header = new Row(false, true);
            Header.AddColumn(new TextColumn(0, "Command", true));
            Header.AddColumn(new TextColumn(1, "Text", false));
            Header.AddColumn(new TextColumn(2, "Mode", false));
            Header.AddColumn(new TextColumn(3, "AutoInterval", false));
            Header.AddColumn(new TextColumn(4, "Stream", true));
            Header.AddColumn(new TextColumn(5, ""));
            Header.AddColumn(new TextColumn(6, ""));
            table.AddRow(Header);
            foreach (var command in _context.StreamCommand.Include(sc => sc.stream))
            {
                Row commandrow = new Row();
                commandrow.AddColumn(new TextColumn(0, command.name));
                commandrow.AddColumn(new TextColumn(1, command.response));
                commandrow.AddColumn(new TextColumn(2, command.Mode.ToString()));
                commandrow.AddColumn(new TextColumn(3, command.AutoInverval.ToString()));
                commandrow.AddColumn(new TextColumn(4, command.stream.StreamName));
                commandrow.AddColumn(new ObjectDeleteColumn(5, "Löschen", command.DeleteLink(), command.DeleteText())) ;
                commandrow.AddColumn(new StreamCommandEditColumn(6, "Edit",command.ID)) ;

                table.AddRow(commandrow);
            }

            return table.getJson();
        }
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<String> GetCreateData()
        {
            try
            {
                var data = new BobDeathmic.ViewModels.ReactDataClasses.Other.StreamCommandEditData(_context.StreamModels.ToList());
                return data.ToJSON();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }

        }
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<String> GetEditData(int streamCommandID)
        {
            try
            {
                var streamcommand = _context.StreamCommand.Where(x => x.ID == streamCommandID).Include(x => x.stream).FirstOrDefault();
                if (streamcommand != null)
                {
                    var data = new BobDeathmic.ViewModels.ReactDataClasses.Other.StreamCommandEditData(streamcommand, _context.StreamModels.ToList());
                    return data.ToJSON();
                }
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return "";
            }

        }
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<bool> CreateCommand(string Name, string Response, StreamCommandMode Mode, int StreamID)
        {
            try
            {
                var command = new StreamCommand();
                command.name = Name;
                command.response = Response;
                command.Mode = Mode;
                command.streamID = StreamID;
                _context.StreamCommand.Add(command);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;

        }
        [HttpPost]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<bool> SaveCommand(int ID, string Name,string Response, StreamCommandMode Mode, int StreamID)
        {
            try
            {
                var command = _context.StreamCommand.Where(x => x.ID == ID).FirstOrDefault();
                if (command != null)
                {
                    command.name = Name;
                    command.response = Response;
                    command.Mode = Mode;
                    command.streamID = StreamID;
                    _context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;

        }
        // GET: StreamCommands/Create
        public IActionResult Create()
        {
            ViewData["Streams"] = _context.StreamModels.ToList();
            ViewData["Modes"] = new List<StreamCommandMode> { StreamCommandMode.Manual, StreamCommandMode.Auto };
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
        [HttpGet]
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<bool> Delete(int CommandID)
        {
            if (CommandID == null)
            {
                return false;
            }

            var streamCommand = await _context.StreamCommand
                .FirstOrDefaultAsync(m => m.ID == CommandID);
            if (streamCommand != null)
            {
                _context.StreamCommand.Remove(streamCommand);
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        private bool StreamCommandExists(int id)
        {
            return _context.StreamCommand.Any(e => e.ID == id);
        }
    }
}
