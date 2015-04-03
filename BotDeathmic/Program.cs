//using DeathmicChatbot.StreamInfo.Hitbox;
//using DeathmicChatbot.StreamInfo.Twitch;
using DeathmicChatbot.Exceptions;
using DeathmicChatbot.IRC;
//using DeathmicChatbot.Interfaces;
using DeathmicChatbot.Properties;
//using RestSharp;
using IrcDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using IrcDotNet.Ctcp;

namespace DeathmicChatbot
{
    class Program
    {
        static IrcClient QuakeClient;
        public static BotDeathmic bot = null;
        static void Main(string[] args)
        {
            
            try
            {
                bot = new BotDeathmic();
                bot.Connect(Settings.Default.Server,bot.RegistrationInfo);
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
                while(true)
                {
                   
                }
                // bot.Run starts console interface with input for commands not really needed
                //bot.Run();
            }catch(Exception ex)
            {
                ConsoleUtilities.WriteError("Fatal error: " + ex.Message);
                Environment.ExitCode = 1;
            }
            finally
            {
                if (bot != null)
                    bot.Dispose();
            }

        }
    }
}
