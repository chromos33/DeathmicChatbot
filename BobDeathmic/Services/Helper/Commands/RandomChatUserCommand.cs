using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.Data;
using BobDeathmic.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BobDeathmic.Services.Helper.Commands
{
    public class RandomChatUserRegisterCommand : IfCommand
    {
        public string Trigger => "!registerraffle";
        public string alias => "!registername";
        public string Description => "Registers User To Raffle";

        public string Category => "stream";

        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            return CommandEventType.None;
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            return "";
        }

        public async Task<string> ExecuteWhisperCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            if(args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(alias))
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    if(_context.RandomChatUser.Where(x => x.ChatUser == args["username"] && x.Stream == args["channel"]).Count() == 0)
                    {
                        RandomChatUser tmp = new RandomChatUser();
                        tmp.ChatUser = args["username"];
                        tmp.Stream = args["channel"];
                        if(_context.RandomChatUser.Where(x => x.Stream == args["channel"]).Count() == 0)
                        {
                            tmp.Sort = 1;
                        }
                        else
                        {
                            tmp.Sort = _context.RandomChatUser.Where(x => x.Stream == args["channel"]).Max(t => t.Sort) + 1;
                        }
                        _context.RandomChatUser.Add(tmp);
                        _context.SaveChanges();
                        return "You were added.";
                    }
                    else
                    {
                        return "Already in the List";
                    }
                }
            }
            return "";
        }
    }
    public class PickNextChatUserForNameCommand : IfCommand
    {
        public string Trigger => "!next";
        public string alias => "!next";

        public string Description => "Outputs next Member in List";

        public string Category => "stream";

        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            return CommandEventType.None;
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            if (args["elevatedPermissions"] == "True" && (args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(alias)))
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    if (_context.RandomChatUser.Where(x => x.Stream == args["channel"]).Count() > 0)
                    {
                        var user = _context.RandomChatUser.Where(x => x.Stream == args["channel"]).OrderBy(s => s.Sort).First();
                        _context.RandomChatUser.Remove(user);
                        _context.SaveChanges();
                        return user.ChatUser;
                    }
                }
            }
            return "";
        }

        public async Task<string> ExecuteWhisperCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            return "";
        }
    }
}
