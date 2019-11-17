using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDeathmic.ChatCommands.Args;
using BobDeathmic.ChatCommands.Setup;
using BobDeathmic.Data;
using BobDeathmic.Data.DBModels.Quote;
using BobDeathmic.Data.Enums;
using BobDeathmic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq;
using BobDeathmic.Eventbus;

namespace BobDeathmic.ChatCommands
{
    public class QuoteCommand : ICommand
    {
        public string Trigger => "!quote";
        public string Description => "Add a quote to the current streamer's quote database. !quote [add/delete] [quote/id]";
        public string Category => "stream";

        public string Alias => "";


        private async Task<bool> DeleteQuoteFromStreamer(string sStreamer, int iQuoteId, ApplicationDbContext context)
        {
            var quote = await context.Quotes.FirstOrDefaultAsync(q => q.Streamer == sStreamer && q.Id == iQuoteId);

            if (quote == null)
            {
                return false;
            }

            context.Quotes.Remove(quote);
            await context.SaveChangesAsync();
            return true;
        }

        private async Task<string> GetRandomQuoteFromStreamer(string sStreamer, ApplicationDbContext context)
        {
            var quotes = context.Quotes.Where(q => q.Streamer == sStreamer).ToArray();
            return quotes.Length == 0 ? $"Found no quotes from {sStreamer}." : quotes.RandomSubset(1).First().ToString();
        }

        private async Task<string> GetQuoteFromStreamer(string sStreamer, int iQuoteId, ApplicationDbContext context)
        {
            var quote = await context.Quotes.FirstOrDefaultAsync(q => q.Streamer == sStreamer && q.Id == iQuoteId);
            return quote?.ToString() ?? $"Found no quote with ID {iQuoteId} from {sStreamer}.";
        }

        private async Task<int> AddQuoteToStreamer(string sStreamer, string sQuote, ApplicationDbContext context)
        {
            var quote = new Quote
            {
                Streamer = sStreamer,
                Created = DateTime.Now,
                Text = sQuote
            };
            await context.Quotes.AddAsync(quote);
            await context.SaveChangesAsync();
            return quote.Id;
        }

        public bool ChatSupported(ChatType chat)
        {
            return chat == ChatType.Twitch;
        }
        public bool isCommand(string str)
        {
            return str.ToLower().StartsWith(Trigger) || str.ToLower().StartsWith(Alias);
        }

        public async Task<ChatCommandOutput> execute(ChatCommandInputArgs args, IServiceScopeFactory scopeFactory)
        {
            ChatCommandOutput output = new ChatCommandOutput();
            output.ExecuteEvent = false;
            output.Type = Eventbus.EventType.CommandResponseReceived;
            output.EventData = new CommandResponseArgs()
            {
                 Channel = args.ChannelName,
                 Chat = args.Type,
                 MessageType = MessageType.ChannelMessage,
                 Sender = args.Sender
            };
            if (!args.Message.ToLower().StartsWith(Trigger) || args.Type != ChatType.Twitch)
            {
                return output;
            }

            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var saMessageSplit = args.Message.Split(" ");

                // !quote without any arguments prints a random quote of the current streamer.
                if (saMessageSplit.Length == 1)
                {
                    output.ExecuteEvent = true;
                    output.EventData.Message = await GetRandomQuoteFromStreamer(args.ChannelName, context);
                    return output;
                }

                // Non mods can only print quotes, not add or delete.
                if (!args.elevatedPermissions)
                {
                    return output;
                }

                int iQuoteId;

                switch (saMessageSplit[1].ToLower())
                {
                    case "add":
                        if (saMessageSplit.Length == 2)
                        {
                            output.EventData.Message = "Usage: !quote add <add funny quote here>";
                        }
                        else
                        {
                            iQuoteId = await AddQuoteToStreamer(args.ChannelName, string.Join(" ", saMessageSplit.Skip(2)), context);
                            output.EventData.Message = await GetQuoteFromStreamer(args.ChannelName, iQuoteId, context);
                        }
                        break;
                    case "delete":
                        if (saMessageSplit.Length == 2)
                        {
                            output.EventData.Message = "Usage: !quote delete <quote ID here>";
                        }

                        if (!int.TryParse(saMessageSplit[2], out iQuoteId))
                        {
                            output.EventData.Message = $"'{saMessageSplit[2]}' is not a quote ID.";
                        }

                        output.EventData.Message = await DeleteQuoteFromStreamer(args.ChannelName, iQuoteId, context)
                            ? "Quote deleted."
                            : $"Quote ID {iQuoteId} not found.";
                        break;
                    default:
                        if (!int.TryParse(saMessageSplit[1], out iQuoteId))
                        {
                            output.EventData.Message = $"'{saMessageSplit[1]}' is not a quote ID.";
                        }

                        output.EventData.Message = await GetQuoteFromStreamer(args.ChannelName, iQuoteId, context);
                        break;
                }
                return output;
            }
        }
    }
}