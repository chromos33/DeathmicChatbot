using BobDeathmic.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BobDeathmic.Controllers
{
    public class ExportForRemasterController : Controller
    {


        private readonly ApplicationDbContext _context;

        public ExportForRemasterController(ApplicationDbContext context)
        {
            _context = context;
        }
        [Authorize(Roles = "Dev,Admin")]
        public IActionResult Index()
        {
            List<dynamic> Members = new List<dynamic>();
            foreach(var user in _context.Users)
            {
                Members.Add(new { UserName = user.ChatUserName });
            }
            List<dynamic> Streams = new List<dynamic>();
            foreach (var stream in _context.StreamModels.Where(x => x.Type == Data.Enums.Stream.StreamProviderTypes.Twitch))
            {
                Streams.Add(new { StreamName = stream.StreamName });
            }
            List<dynamic> Subscriptions = new List<dynamic>();
            foreach (var subscription in _context.StreamSubscriptions.Include(x => x.Stream).Include(x => x.User))
            {
                if(subscription.Stream.Type == Data.Enums.Stream.StreamProviderTypes.Twitch)
                {
                    bool isSubscribed = subscription.Subscribed == Data.Enums.Stream.SubscriptionState.Subscribed;
                    Subscriptions.Add(new { isSubscribed = isSubscribed, UserName = subscription.User.ChatUserName, StreamName = subscription.Stream.StreamName });
                }
            }

            var output = new { Members = Members, Streams = Streams, Subscriptions = Subscriptions };

            string ContentString = JsonConvert.SerializeObject(output);
            var content = Encoding.ASCII.GetBytes(ContentString); ;
            var contentType = "APPLICATION/octet-stream";
            var fileName = "test.json";
            return File(content, contentType, fileName);
        }
    }
}
