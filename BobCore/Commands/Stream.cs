using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BobCore.DataClasses;

namespace BobCore.Commands
{
    class AddStream : IFCommand
    {
        private string Trigger = "!addstream";
        public string sTrigger { get { return Trigger; } }
        private string[] Requirements = { "User", "Stream" };
        public string description { get { return "Fügt Stream der Liste hinzu"; } }
        public string category { get { return "Stream"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        List<DataClasses.internalStream> StreamList;
        List<DataClasses.User> UserList;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return "!addstream [streamname]";
                    }
                    else
                    {
                        var Stream = StreamList.Where(x => x.sChannel.ToLower() == @params[0].ToLower()).FirstOrDefault();
                        if (Stream == null)
                        {
                            internalStream newStream = new internalStream();
                            newStream.sChannel = @params[0];
                            newStream.bRunning = false;
                            StreamList.Add(newStream);
                            Administrative.XMLFileHandler.writeFile(StreamList, "Streams");
                            var FilteredList = UserList.Where(x => !x.hasStream(@params[0]));
                            bool added = false;
                            foreach (var user in FilteredList)
                            {
                                added = true;
                                user.addStream(@params[0], true);
                            }
                            if (added)
                            {
                                Administrative.XMLFileHandler.writeFile(UserList, "Users");
                            }
                            return @params[0] + " has been added to the list";
                        }
                        else
                        {
                            return @params[0] + " already exists";
                        }
                    }
                }
            }
            return "";

        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                UserList = _DataList;
            }
            if (type == "DataClasses.Stream")
            {
                StreamList = _DataList;
            }
        }
    }
    class DelStream : IFCommand
    {
        private string Trigger = "!delstream";
        public string sTrigger { get { return Trigger; } }
        private string[] Requirements = { "User", "Stream" };
        public string description { get { return "Löscht Stream aus der Liste"; } }
        public string category { get { return "Stream"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        List<DataClasses.internalStream> StreamList;
        List<DataClasses.User> UserList;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return "!delstream [streamname]";
                    }
                    else
                    {
                        var Stream = StreamList.Where(x => x.sChannel.ToLower() == @params[0].ToLower()).FirstOrDefault();
                        if (Stream == null)
                        {
                            return @params[0] + " does not exist in the list";
                        }
                        else
                        {
                            int index = StreamList.IndexOf(Stream);
                            StreamList.RemoveAt(index);
                            Administrative.XMLFileHandler.writeFile(StreamList, "Streams");
                            return @params[0] + " removed from the list";
                        }
                    }
                }
            }
            return "";

        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                UserList = _DataList;
            }
            if (type == "DataClasses.Stream")
            {
                StreamList = _DataList;
            }
        }
    }
    class StreamCheck : IFCommand
    {
        private string Trigger = "!streamcheck";
        public string sTrigger { get { return Trigger; } }
        private string[] Requirements = { "User", "Stream" };
        public string description { get { return "Listet alle momentan laufenden Streams"; } }
        public string category { get { return "Stream"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        List<DataClasses.internalStream> StreamList;
        List<DataClasses.User> UserList;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            Console.WriteLine("!streamcheck".Contains("!streamcheck"));

            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return "Ernsthaft?";
                    }
                }
                if (@params.Count() == 0)
                {
                    string output = "";
                    var Streams = StreamList.Where(x => x.bRunning == true);
                    var user = UserList.Where(x => x.isUser(username.ToLower())).FirstOrDefault();
                    if (user != null)
                    {
                        foreach (var stream in Streams)
                        {
                            if (user.isSubscribed(stream.sChannel))
                            {
                                output += stream.sChannel + " @ " + stream.sUrl + Environment.NewLine;
                            }
                        }
                    }
                    if (output == "")
                    {
                        return "no stream running";
                    }
                    return output;
                }
            }
            return "";

        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                UserList = _DataList;
            }
            if (type == "DataClasses.Stream")
            {
                StreamList = _DataList;
            }
        }
    }
    class SubscribableStreams : IFCommand
    {
        private string Trigger = "!subscribablestreams";
        private string[] Requirements = { "User", "Stream" };
        public string description { get { return "Listet alle Streams"; } }
        public string category { get { return "Stream"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        List<DataClasses.internalStream> StreamList;
        List<DataClasses.User> UserList;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return "Ernsthaft?";
                    }
                }
                string output = "";
                foreach (internalStream stream in StreamList)
                {
                    output += stream.sChannel + Environment.NewLine;
                }
                return output;

            }
            return "";

        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                UserList = _DataList;
            }
            if (type == "DataClasses.Stream")
            {
                StreamList = _DataList;
            }
        }
    }
    class ManageStreamRelay : IFCommand
    {
        private string Trigger = "!managestreamrelay";
        private string[] Requirements = { "User", "Stream" };
        public string description { get { return "Aktiviert bei einem Stream die Relay Funktion"; } }
        public string category { get { return "Stream"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        List<DataClasses.internalStream> StreamList;
        List<DataClasses.User> UserList;

        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0].Contains("help"))
                    {
                        return "!managestreamrelay [streamname] [targetchannel(Discord)] [(optional default 1) twoway (0/1)]";
                    }
                    if (@params.Count() == 2)
                    {
                        int twoway = 1;
                        if (@params.Count() == 3)
                        {
                            twoway = Int32.Parse(@params[2]);
                        }
                        string streamname = @params[0];
                        string targetchannel = @params[1];
                        var stream = StreamList.Where(x => x.sChannel.ToLower() == @params[0].ToLower()).FirstOrDefault();
                        if (stream != null)
                        {
                            stream.sTargetrelaychannel = @params[1];
                            stream.bTwoway = twoway != 0;
                            Administrative.XMLFileHandler.writeFile(StreamList, "Streams");
                            return "Stream relay added";
                        }
                        return "Stream existiert noch nicht";
                    }
                }
            }
            return "";
        }

        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                UserList = _DataList;
            }
            if (type == "DataClasses.Stream")
            {
                StreamList = _DataList;
            }
        }
    }
    class MoveRelay : IFCommand
    {
        private string Trigger = "!moverelay";
        public string sTrigger { get { return Trigger; } }
        private string[] Requirements = { "User", "Stream" };
        public string description { get { return "Verschiebt das Relay eines Streams temporär in einen anderen Channel"; } }
        public string category { get { return "Stream"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        List<DataClasses.internalStream> StreamList;
        List<DataClasses.User> UserList;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return "!moverelay [streamname] [channelname]";
                    }
                    else
                    {
                        if (@params.Count() == 2)
                        {
                            var Stream = StreamList.Where(x => x.sChannel.ToLower() == @params[0].ToLower()).FirstOrDefault();
                            if (Stream != null)
                            {
                                Stream.MoveRelay(@params[1]);
                                return @params[0] + "stream temporarily moved to " + @params[1];
                            }
                            else
                            {
                                return @params[0] + "stream does not exist";
                            }
                        }
                        return "!moverelay [streamname] [channelname]";

                    }
                }
            }
            return "";

        }

        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                UserList = _DataList;
            }
            if (type == "DataClasses.Stream")
            {
                StreamList = _DataList;
            }
        }
    }
}
