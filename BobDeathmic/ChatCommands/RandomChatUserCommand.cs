using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.ChatCommands.Args;
using BobDeathmic.ChatCommands.Setup;
using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.Commands;
using BobDeathmic.Data.Enums;
using BobDeathmic.Eventbus;
using BobDeathmic.Models;
using Microsoft.Extensions.DependencyInjection;

namespace BobDeathmic.ChatCommands
{
    public class RandomChatUserRegisterCommand : ICommand
    {
        public string Trigger => "!registerraffle";
        public string Alias => "!registername";
        public string Description => "Registers User To Raffle";

        public string Category => "stream";

        public bool ChatSupported(ChatType chat)
        {
            return chat == ChatType.Twitch;
        }

        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            return CommandEventType.None;
        }

        public async Task<ChatCommandOutput> execute(ChatCommandInputArgs args, IServiceScopeFactory scopefactory)
        {
            ChatCommandOutput output = new ChatCommandOutput();
            output.ExecuteEvent = true;
            output.Type = Eventbus.EventType.CommandResponseReceived;
            string message = "";
            using (var scope = scopefactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (_context.RandomChatUser.Where(x => x.ChatUser.ToLower() == args.Sender.ToLower() && x.Stream.ToLower() == args.ChannelName.ToLower()).Count() == 0)
                {
                    RandomChatUser tmp = new RandomChatUser();
                    tmp.ChatUser = args.Sender;
                    tmp.Stream = args.ChannelName;
                    if (_context.RandomChatUser.Where(x => x.Stream.ToLower() == args.ChannelName.ToLower()).Count() == 0)
                    {
                        tmp.Sort = 1;
                    }
                    else
                    {
                        tmp.Sort = _context.RandomChatUser.Where(x => x.Stream.ToLower() == args.ChannelName.ToLower()).Max(t => t.Sort) + 1;
                    }
                    _context.RandomChatUser.Add(tmp);
                    _context.SaveChanges();
                    message = "You were added.";
                }
                else
                {
                    message = "Already in the List";
                }
            }
            output.EventData = new CommandResponseArgs(args.Type, message, MessageType.PrivateMessage, EventType.CommandResponseReceived, args.Sender,args.ChannelName);
            return output;
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            return "";
        }

        public async Task<string> ExecuteWhisperCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            if(args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(Alias))
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
        public bool isCommand(string str)
        {
            return str.ToLower().StartsWith(Trigger) || str.ToLower().StartsWith(Alias);
        }
    }
    public class PickNextChatUserForNameCommand : ICommand
    {
        public string Trigger => "!next";
        public string Alias => "!next";

        public string Description => "Outputs next Member in List";

        public string Category => "stream";

        public bool ChatSupported(ChatType chat)
        {
            return chat == ChatType.Twitch;
        }
        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            return CommandEventType.None;
        }

        public async Task<ChatCommandOutput> execute(ChatCommandInputArgs args, IServiceScopeFactory scopeFactory)
        {
            ChatCommandOutput output = new ChatCommandOutput();
            output.ExecuteEvent = false;
            output.Type = Eventbus.EventType.CommandResponseReceived;
            string message = "No User found/in List";
            if (args.elevatedPermissions && (args.Message.ToLower().StartsWith(Trigger) || args.Message.ToLower().StartsWith(Alias)))
            {
                output.ExecuteEvent = true;
                using (var scope = scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var delete = _context.RandomChatUser.Where(x => x.Stream == args.ChannelName && x.lastchecked).FirstOrDefault();
                    if (delete != null)
                    {
                        _context.RandomChatUser.Remove(delete);
                        _context.SaveChanges();
                    }
                    if (_context.RandomChatUser.Where(x => x.Stream == args.ChannelName).Count() > 0)
                    {
                        var user = _context.RandomChatUser.Where(x => x.Stream == args.ChannelName).OrderBy(s => s.Sort).First();
                        user.lastchecked = true;
                        _context.SaveChanges();
                        message = user.ChatUser;
                    }
                }
                output.EventData = new CommandResponseArgs(args.Type, message, MessageType.ChannelMessage, EventType.CommandResponseReceived, args.Sender,args.ChannelName);

            }
            return output;
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            if (args["elevatedPermissions"] == "True" && (args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(Alias)))
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
        public bool isCommand(string str)
        {
            return str.ToLower().StartsWith(Trigger) || str.ToLower().StartsWith(Alias);
        }
    }
    public class PickNextRandChatUserForNameCommand : ICommand
    {
        public string Trigger => "!randnext";
        public string Alias => "!randnext";
        private Random rnd = new Random();

        public string Description => "Outputs weighted random Member in List";

        public string Category => "stream";

        public bool ChatSupported(ChatType chat)
        {
            return chat == ChatType.Twitch;
        }
        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            return CommandEventType.None;
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            if (args["elevatedPermissions"] == "True" && (args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(Alias)))
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

        public async Task<ChatCommandOutput> execute(ChatCommandInputArgs args, IServiceScopeFactory scopeFactory)
        {
            ChatCommandOutput output = new ChatCommandOutput();
            output.ExecuteEvent = false;
            output.Type = Eventbus.EventType.CommandResponseReceived;
            string message = "No User found/in List";
            if (args.elevatedPermissions && (args.Message.ToLower().StartsWith(Trigger) || args.Message.ToLower().StartsWith(Alias)))
            {
                output.ExecuteEvent = true;
                using (var scope = scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var delete = _context.RandomChatUser.Where(x => x.Stream == args.ChannelName && x.lastchecked).FirstOrDefault();
                    if (delete != null)
                    {
                        _context.RandomChatUser.Remove(delete);
                        _context.SaveChanges();
                    }
                    if (_context.RandomChatUser.Where(x => x.Stream == args.ChannelName).Count() > 0)
                    {
                        if (_context.RandomChatUser.Where(x => x.Stream == args.ChannelName).Count() == 1)
                        {
                            _context.RandomChatUser.Where(x => x.Stream == args.ChannelName).FirstOrDefault().lastchecked = true;
                            _context.SaveChanges();
                            message = _context.RandomChatUser.Where(x => x.Stream == args.ChannelName).FirstOrDefault().ChatUser;
                        }
                        else
                        {
                            var users = _context.RandomChatUser.Where(x => x.Stream == args.ChannelName);
                            var count = users.Count();
                            List<string> Names = new List<string>();
                            foreach (RandomChatUser usertemplate in users)
                            {
                                for (int i = count; i > 0; i--)
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
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                            message = nextuser;
                        }
                        
                    }
                }
                output.EventData = new CommandResponseArgs(args.Type, message, MessageType.ChannelMessage, EventType.CommandResponseReceived, args.Sender, args.ChannelName);
            }
            return output;
        }
        public bool isCommand(string str)
        {
            return str.ToLower().StartsWith(Trigger) || str.ToLower().StartsWith(Alias);
        }
    }
    public class ListRandUsersInListCommand : ICommand
    {
        public string Trigger => "!list";
        public string Alias => "!list";
        private Random rnd = new Random();

        public string Description => "Outputs List";

        public string Category => "stream";

        public bool ChatSupported(ChatType chat)
        {
            return chat == ChatType.Twitch;
        }
        public async Task<CommandEventType> EventToBeTriggered(Dictionary<string, string> args)
        {
            return CommandEventType.None;
        }

        public async Task<string> ExecuteCommandIfApplicable(Dictionary<string, string> args, IServiceScopeFactory scopeFactory)
        {
            if ((args["message"].ToLower().StartsWith(Trigger) || args["message"].ToLower().StartsWith(Alias)))
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

        public async Task<ChatCommandOutput> execute(ChatCommandInputArgs args, IServiceScopeFactory scopeFactory)
        {
            ChatCommandOutput output = new ChatCommandOutput();
            output.ExecuteEvent = false;
            output.Type = Eventbus.EventType.CommandResponseReceived;
            string message = "No User found/in List";
            if (args.elevatedPermissions && (args.Message.ToLower().StartsWith(Trigger) || args.Message.ToLower().StartsWith(Alias)))
            {
                output.ExecuteEvent = true;
                using (var scope = scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    if (_context.RandomChatUser.Where(x => x.Stream == args.ChannelName).Count() > 0)
                    {
                        message = "Users in List: ";
                        foreach (var user in _context.RandomChatUser.Where(x => x.Stream == args.ChannelName).OrderBy(s => s.Sort))
                        {
                            message += user.ChatUser + "\n";
                        }
                    }
                }
                output.EventData = new CommandResponseArgs(args.Type, message, MessageType.ChannelMessage, EventType.CommandResponseReceived, args.Sender, args.ChannelName);
            }
            return output;
        }
        public bool isCommand(string str)
        {
            return str.ToLower().StartsWith(Trigger) || str.ToLower().StartsWith(Alias);
        }
    }
    public class SkipLastRandUserCommand : ICommand
    {
        public string Trigger => "!skip";
        public string Alias => "!skip";
        private Random rnd = new Random();

        public string Description => "Skips last selected user";

        public string Category => "stream";

        public bool ChatSupported(ChatType chat)
        {
            return chat == ChatType.Twitch;
        }
        public async Task<ChatCommandOutput> execute(ChatCommandInputArgs args, IServiceScopeFactory scopeFactory)
        {
            ChatCommandOutput output = new ChatCommandOutput();
            output.ExecuteEvent = false;
            output.Type = Eventbus.EventType.CommandResponseReceived;
            string message = "No skippable user found";
            if (args.elevatedPermissions && (args.Message.ToLower().StartsWith(Trigger) || args.Message.ToLower().StartsWith(Alias)))
            {
                output.ExecuteEvent = true;
                using (var scope = scopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    if (_context.RandomChatUser.Where(x => x.Stream == args.ChannelName && x.lastchecked).Count() > 0)
                    {
                        var skippeduser = _context.RandomChatUser.Where(x => x.Stream == args.ChannelName && x.lastchecked).FirstOrDefault();
                        if (skippeduser != null)
                        {
                            _context.RandomChatUser.Remove(skippeduser);
                            RandomChatUser tmp = new RandomChatUser();
                            tmp.ChatUser = skippeduser.ChatUser;
                            tmp.lastchecked = false;
                            if (_context.RandomChatUser.Where(x => x.Stream == args.ChannelName).Count() == 0)
                            {
                                tmp.Sort = 1;
                            }
                            else
                            {
                                tmp.Sort = _context.RandomChatUser.Where(x => x.Stream == args.ChannelName).Max(t => t.Sort) + 1;
                            }
                            tmp.Stream = skippeduser.Stream;
                            _context.RandomChatUser.Add(tmp);
                            _context.SaveChanges();
                            message = "User skipped";
                        }
                    }
                }
                output.EventData = new CommandResponseArgs(args.Type, message, MessageType.ChannelMessage, EventType.CommandResponseReceived, args.Sender, args.ChannelName);
            }
            return output;
        }
        public bool isCommand(string str)
        {
            return str.ToLower().StartsWith(Trigger) || str.ToLower().StartsWith(Alias);
        }
    }
}
