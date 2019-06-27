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
                    var delete = _context.RandomChatUser.Where(x => x.Stream == args["channel"] && x.lastchecked).FirstOrDefault();
                    if (delete != null)
                    {
                        _context.RandomChatUser.Remove(delete);
                        _context.SaveChanges();
                    }
                    if (_context.RandomChatUser.Where(x => x.Stream == args["channel"]).Count() > 0)
                    {
                        var user = _context.RandomChatUser.Where(x => x.Stream == args["channel"]).OrderBy(s => s.Sort).First();
                        user.lastchecked = true;
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
    public class PickNextRandChatUserForNameCommand : IfCommand
    {
        public string Trigger => "!randnext";
        public string alias => "!randnext";
        private Random rnd = new Random();

        public string Description => "Outputs weighted random Member in List";

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
                    var delete = _context.RandomChatUser.Where(x => x.Stream == args["channel"] && x.lastchecked).FirstOrDefault();
                    if(delete != null)
                    {
                        _context.RandomChatUser.Remove(delete);
                        _context.SaveChanges();
                    }
                    if (_context.RandomChatUser.Where(x => x.Stream == args["channel"]).Count() > 0)
                    {
                        if(_context.RandomChatUser.Where(x => x.Stream == args["channel"]).Count() == 1)
                        {
                            _context.RandomChatUser.Where(x => x.Stream == args["channel"]).FirstOrDefault().lastchecked = true;
                            _context.SaveChanges();
                            return _context.RandomChatUser.Where(x => x.Stream == args["channel"]).FirstOrDefault().ChatUser;
                        }
                        var users = _context.RandomChatUser.Where(x => x.Stream == args["channel"]);
                        var count = users.Count();
                        List<string> Names = new List<string>();
                        foreach(RandomChatUser usertemplate in users)
                        {
                            for(int i = count; i > 0;i--)
                            {
                                Names.Add(usertemplate.ChatUser);
                            }
                            count--;
                        }
                        var nextuser = Names[rnd.Next(Names.Count() + 1)];
                        try
                        {
                            _context.RandomChatUser.Where(x => x.ChatUser == nextuser).First().lastchecked = true;
                            _context.SaveChanges();
                        }catch(Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                        return nextuser;
                    }
                    return "Liste ist leer";
                }
            }
            return "";
        }

        public async Task<string> ExecuteWhisperCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            return "";
        }
    }
    public class ListRandUsersInListCommand : IfCommand
    {
        public string Trigger => "!list";
        public string alias => "!list";
        private Random rnd = new Random();

        public string Description => "Outputs List";

        public string Category => "stream";

        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            return CommandEventType.None;
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            if ((args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(alias)))
            {
                using (var scope = scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    if (_context.RandomChatUser.Where(x => x.Stream == args["channel"]).Count() > 0)
                    {
                        string message = "Users in List: ";
                        foreach (var user in _context.RandomChatUser.Where(x => x.Stream == args["channel"]).OrderBy(s => s.Sort))
                        {
                            message += user.ChatUser + "\n";
                        }
                        return message;
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
    public class SkipLastRandUserCommand : IfCommand
    {
        public string Trigger => "!skip";
        public string alias => "!skip";
        private Random rnd = new Random();

        public string Description => "Skips last selected user";

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
                    if (_context.RandomChatUser.Where(x => x.Stream == args["channel"] && x.lastchecked).Count() > 0)
                    {
                        var skippeduser = _context.RandomChatUser.Where(x => x.Stream == args["channel"] && x.lastchecked).FirstOrDefault();
                        if (skippeduser != null)
                        {
                            _context.RandomChatUser.Remove(skippeduser);
                            RandomChatUser tmp = new RandomChatUser();
                            tmp.ChatUser = skippeduser.ChatUser;
                            tmp.lastchecked = false;
                            if (_context.RandomChatUser.Where(x => x.Stream == args["channel"]).Count() == 0)
                            {
                                tmp.Sort = 1;
                            }
                            else
                            {
                                tmp.Sort = _context.RandomChatUser.Where(x => x.Stream == args["channel"]).Max(t => t.Sort) + 1;
                            }
                            tmp.Stream = skippeduser.Stream;
                            _context.RandomChatUser.Add(tmp);
                            _context.SaveChanges();
                            return "User skipped";
                        }
                    }
                    return "No skippable user found";
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
