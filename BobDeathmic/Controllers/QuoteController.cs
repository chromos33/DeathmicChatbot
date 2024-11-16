using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BobDeathmic.ViewModels.ReactDataClasses.Table;
using BobDeathmic.ViewModels.ReactDataClasses.Table.Columns;

namespace BobDeathmic.Controllers
{
    public class QuoteController : Controller
    {
        private readonly ApplicationDbContext _context;
        public QuoteController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "User,Dev,Admin")]
        public async Task<string> QuotesData()
        {
            Table table = new Table();
            Row Header = new Row(false, true);
            Header.AddColumn(new TextColumn(0,"Streamer",true));
            Header.AddColumn(new TextColumn(1,"Erstellt",false));
            Header.AddColumn(new TextColumn(2,"Quote", false));
            Header.AddColumn(new TextColumn(3,"ID", false));
            table.AddRow(Header);
            foreach(var quote in _context.Quotes)
            {
                Row quoterow = new Row();
                quoterow.AddColumn(new TextColumn(0, quote.Streamer));
                quoterow.AddColumn(new TextColumn(1, quote.Created.ToString("dd MMMM yyyy")));
                quoterow.AddColumn(new TextColumn(2, quote.Text));
                quoterow.AddColumn(new TextColumn(3, quote.Id.ToString()));
                table.AddRow(quoterow);
            }

            return table.getJson();
        }
    }
}