using BobDeathmic.ChatCommands.Args;
using BobDeathmic.ChatCommands.Setup;
using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.GiveAway.manymany;
using BobDeathmic.Data.Enums;
using BobDeathmic.Eventbus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.ChatCommands
{
    public class GiveAwayApply : ICommand
    {
        public string Trigger => "!Gapply";

        public string Description => "Command um an momentaner Verlosung teilzunehmen";

        public string Category => "GiveAway";

        public string Alias => "!Gapply";

        public bool ChatSupported(ChatType chat)
        {
            return chat == ChatType.Discord;
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
                var GiveAwayItem = _context.GiveAwayItems.Include(x => x.Applicants).Where(x => x.current).FirstOrDefault();
                if (GiveAwayItem.Applicants == null)
                {
                    GiveAwayItem.Applicants = new List<User_GiveAwayItem>();
                }
                var user = _context.ChatUserModels.Where(x => x.ChatUserName.ToLower() == args.Sender.ToLower()).FirstOrDefault();
                if (GiveAwayItem.Applicants.Where(x => x.UserID == user.Id).Count() == 0)
                {

                    var item = _context.GiveAwayItems.Where(x => x.current).FirstOrDefault();
                    if (user != null && item != null)
                    {
                        User_GiveAwayItem relation = new User_GiveAwayItem(user, item);
                        relation.User = user;
                        if (user.AppliedTo == null)
                        {
                            user.AppliedTo = new List<User_GiveAwayItem>();
                        }
                        if (item.Applicants == null)
                        {
                            item.Applicants = new List<User_GiveAwayItem>();
                        }
                        user.AppliedTo.Add(relation);
                        item.Applicants.Add(relation);
                        message= "Teilnahme erfolgreich";
                    }
                    else
                    {
                        message = "Gibt nichs zum teilnehmen";
                    }
                }
                else
                {
                    message = "Nimmst schon teil.";
                }
                _context.SaveChanges();
            }
            output.EventData = new CommandResponseArgs(
                    ChatType.Discord,
                    message,
                    MessageType.PrivateMessage,
                    args.Sender,
                    args.ChannelName
                );
            return output;
        }

        public bool isCommand(string message)
        {
            return message.ToLower().StartsWith(Trigger.ToLower());
        }
    }
    public class GiveAwayCease : ICommand
    {
        public string Trigger => "!Gcease";

        public string Description => "Command um momentaner Teilnahme zurückzuziehen";

        public string Category => "GiveAway";

        public string Alias => "!Gcease";

        public bool ChatSupported(ChatType chat)
        {
            return chat == ChatType.Discord;
        }

        public async Task<ChatCommandOutput> execute(ChatCommandInputArgs args, IServiceScopeFactory scopefactory)
        {
            ChatCommandOutput output = new ChatCommandOutput();
            output.ExecuteEvent = false;
            output.Type = Eventbus.EventType.CommandResponseReceived;
            string message = "";
            using (var scope = scopefactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = _context.ChatUserModels.Where(x => x.ChatUserName.ToLower() == args.Sender.ToLower()).FirstOrDefault();
                var remove = _context.User_GiveAway.Where(x => x.UserID == user.Id).FirstOrDefault();
                if (remove != null)
                {
                    _context.User_GiveAway.Remove(remove);
                    _context.SaveChanges();
                }
            }
            return output;
        }

        public bool isCommand(string message)
        {
            return message.ToLower().StartsWith(Trigger.ToLower());
        }
    }
}
