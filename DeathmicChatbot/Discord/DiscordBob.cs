using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DeathmicChatbot.Discord
{
    class DiscordBob
    {
        private DiscordClient bot;
        public void Connect()
        {
            var bot = new DiscordClient();
            bot.MessageReceived += Message_Received;
            bot.ExecuteAndWait(async () =>
            {
                await bot.Connect("MjYwMTE2OTExOTczNDY2MTEy.CzhvyA.kpEIti2hVnjNIUccob0ERB4QFTw", TokenType.Bot);

            });
        }

        private void Message_Received(object sender, MessageEventArgs e)
        {
            //Console.WriteLine(e.Message);
            Console.WriteLine(e.Channel);
            CommandFilter(e);
        }
        private void CommandFilter(MessageEventArgs e)
        {
            string messagecontent = e.Message.ToString();
            if(messagecontent.ToLower().Contains("!test"))
            {
                e.Channel.Users.Where(x => x.Name.Contains("chromos33")).First().SendMessage("test");
            }
        }
    }
}
