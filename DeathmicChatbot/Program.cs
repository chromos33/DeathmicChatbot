using DeathmicChatbot.IRC;
using DeathmicChatbot.Properties;
using IrcDotNet;
using System;
using IrcDotNet.Ctcp;
using System.Net;
using System.Threading;
namespace DeathmicChatbot
{
    class Program
    {
        public static BotDeathmic bot = null;
        static void Main(string[] args)
        {
            Console.WriteLine(System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length);
            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
            {
                Environment.Exit(1);
            }
            else
            {
                try
                {
                    ConnectBot();
                    // bot.Run starts console interface with input for commands not really needed
                    //bot.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fatal error: " + ex.Message);
                    Environment.ExitCode = 1;
                }
                finally
                {
                    if (bot != null)
                        bot.Dispose();
                }
            }
        }
        static void ConnectBot()
        {
            bot = new BotDeathmic();
            bot.Connect(Settings.Default.Server, bot.RegistrationInfo);
            //Quakenet connect duration fix
            if (Settings.Default.Server.Contains("quakenet"))
            {
                string quakeservername = null;
                foreach (var _client in bot.Clients)
                {
                    while (_client.ServerName == null)
                    {

                    }
                    if (_client.ServerName.Contains("quakenet"))
                    {
                        quakeservername = _client.ServerName;
                        bot.thisclient = _client;
                        bot.ctcpClient1 = new CtcpClient(_client);
                        bot.ctcpClient1.ClientVersion = bot.clientVersionInfo;
                        bot.ctcpClient1.PingResponseReceived += bot.ctcpClient_PingResponseReceived;
                        bot.ctcpClient1.VersionResponseReceived += bot.ctcpClient_VersionResponseReceived;
                        bot.ctcpClient1.TimeResponseReceived += bot.ctcpClient_TimeResponseReceived;
                        bot.ctcpClient1.ActionReceived += bot.ctcpClient_ActionReceived;
                    }

                }
                var quakeclient = bot.GetClientFromServerNameMask(quakeservername);
                System.Diagnostics.Debug.WriteLine(Properties.Settings.Default.Channel + " " + quakeservername);
                quakeclient.Channels.Join(Properties.Settings.Default.Channel);
            }
            else
            {
                foreach (var _client in bot.Clients)
                {
                    _client.Channels.Join(Properties.Settings.Default.Channel);
                    bot.thisclient = _client;
                    bot.ctcpClient1 = new CtcpClient(_client);
                }
            }
            while (true)
            {
                if(bot != null)
                {
                    if (!bot.thisclient.IsConnected)
                    {
                        bot.Dispose();
                        bot = null;
                    }
                }
                
                if(bot == null)
                {
                    if (CheckForInternetConnection())
                    {
                        // Integrate Self kill if Connection possible but nor irc connection because nick etc is already in use etc
                        Console.WriteLine("Connection possible");
                        ConnectBot();
                        break;
                    }
                    System.Threading.Thread.Sleep(1000);
                }
                Thread.Sleep(5000);
            }
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new System.Net.WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
