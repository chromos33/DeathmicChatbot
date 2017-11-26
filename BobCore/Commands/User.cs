using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BobCore.DataClasses;


namespace BobCore.Commands
{
    class AddMyUser : IFCommand
    {
        private string[] Requirements = { "User", "Stream" };
        public string description { get { return "Fügt sofern noch nicht vorhanden den User beim Bob ein"; } }
        public string category { get { return "User"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return "!addmyuser"; } }
        List<DataClasses.internalStream> StreamList;
        List<DataClasses.User> UserList;

        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                if (UserList.Where(x => x.Name.ToLower() == username.ToLower()).Count() > 0)
                {
                    if (UserList.Where(x => x.Streams.Count() == 0).Count() != 0)
                    {
                        List<Stream> userstreamlist = new List<Stream>();
                        foreach (internalStream _stream in StreamList)
                        {
                            Stream newstream = new Stream();
                            newstream.hourlyannouncement = false;
                            newstream.name = _stream.sChannel;
                            newstream.subscribed = true;
                            userstreamlist.Add(newstream);
                        }
                        var user = UserList.Where(x => x.Streams.Count() == 0 && x.isUser(username)).FirstOrDefault();
                        if (user != null)
                        {
                            user.Streams = userstreamlist;
                        }
                        Administrative.XMLFileHandler.writeFile(UserList, "Users");
                        return "User already exists but had no streams attached";
                    }
                    return "User already exists";
                }
                else
                {
                    User newuser = new User();
                    newuser.Name = username;
                    UserList.Add(newuser);
                    List<Stream> userstreamlist = new List<Stream>();
                    foreach (internalStream _stream in StreamList)
                    {
                        Stream newstream = new Stream();
                        newstream.hourlyannouncement = false;
                        newstream.name = _stream.sChannel;
                        newstream.subscribed = true;
                        userstreamlist.Add(newstream);
                    }
                    var user = UserList.Where(x => x.Streams.Count() == 0 && x.isUser(username)).FirstOrDefault();
                    if (user != null)
                    {
                        user.Streams = userstreamlist;
                    }
                    Administrative.XMLFileHandler.writeFile(UserList, "Users");
                    return "User added";
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
    class ChangeSubcription : IFCommand
    {
        private string[] Requirements = { "User", "Stream" };
        public string description { get { return "Zum ändern deiner Stream Suscriptions (Erhalt/Nicht Erhalt von Stream Notifications)"; } }
        public string category { get { return "User"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return "!changesubscription"; } }
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
                        return "!changesubscription [add/remove/removeall] [streamname]";
                    }
                    if (@params.Count() == 1)
                    {
                        var user = UserList.Where(x => x.isUser(username)).FirstOrDefault();
                        if (user != null)
                        {
                            if (@params[0].Contains("removeall"))
                            {
                                foreach (var stream in user.Streams)
                                {
                                    user.subscribeStream(stream.name, false);
                                }
                                Administrative.XMLFileHandler.writeFile(UserList, "Users");
                                return "lazy sunofa b... :D";
                            }
                        }
                        else
                        {
                            return "!addmyuser anwenden und nochmal probieren";
                        }

                    }
                    if (@params.Count() == 2)
                    {
                        var user = UserList.Where(x => x.isUser(username)).FirstOrDefault();
                        if (user != null)
                        {
                            if (@params[0].Contains("add"))
                            {
                                if (user.isSubscribed(@params[1].ToLower()))
                                {
                                    return "Nothing to do here!";
                                }
                                else
                                {
                                    user.subscribeStream(@params[1].ToLower(), true);
                                    Administrative.XMLFileHandler.writeFile(UserList, "Users");
                                    return "My work is done!";
                                }
                            }
                            if (@params[0].Contains("remove"))
                            {
                                user.subscribeStream(@params[1].ToLower(), false);
                                Administrative.XMLFileHandler.writeFile(UserList, "Users");
                                return "unsubscribed";
                            }
                        }
                        else
                        {
                            return "!addmyuser anwenden und nochmal probieren";
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
    class ChangeGlobalAnnouncment : IFCommand
    {
        public string sTrigger { get { return "!changeglobalannouncment"; } }
        private string[] Requirements = { "User", "Stream" };
        public string description { get { return "Zum aktivieren/deaktivieren der Stündlichen Stream Notification"; } }
        public string category { get { return "User"; } }
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
                    if (@params[0].Contains("help"))
                    {
                        return "!changeglobalannouncment [global/stream] [enable/disable/read] [streamname (only with stream as first parameter)]";
                    }
                    if (@params.Count() == 2)
                    {
                        if (@params[0] == "global" && @params.Count() == 2)
                        {
                            if (@params[1] == "enable")
                            {
                                var Users = UserList.Where(x => x.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase));
                                if (Users.Count() > 0)
                                {
                                    Users.First().globalhourlyannouncement = true;
                                    return "global hourly enabled";
                                }
                            }
                            if (@params[1] == "disable")
                            {
                                var Users = UserList.Where(x => x.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase));
                                if (Users.Count() > 0)
                                {
                                    Users.First().globalhourlyannouncement = false;
                                    return "global hourly disabled";
                                }
                            }
                            if (@params[1] == "read")
                            {
                                var Users = UserList.Where(x => x.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase));
                                if (Users.Count() > 0)
                                {
                                    return Users.First().globalhourlyannouncement.ToString();
                                }
                            }
                            Administrative.XMLFileHandler.writeFile(UserList, "Users");
                        }
                        if (@params[0] == "stream" && @params.Count() == 3)
                        {
                            if (@params[1] == "enable")
                            {
                                var Users = UserList.Where(x => x.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase));
                                if (Users.Count() > 0)
                                {
                                    Users.First().Streams.Where(x => x.name.ToLower() == @params[2]).First().hourlyannouncement = true;
                                    return "stream hourly enabled";
                                }
                            }
                            if (@params[1] == "disable")
                            {
                                var Users = UserList.Where(x => x.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase));
                                if (Users.Count() > 0)
                                {
                                    Users.First().Streams.Where(x => x.name.ToLower() == @params[2]).First().hourlyannouncement = false;
                                    return "stream hourly disabled";
                                }
                            }
                            if (@params[1] == "read")
                            {
                                var Users = UserList.Where(x => x.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase));
                                if (Users.Count() > 0)
                                {
                                    return Users.First().Streams.Where(x => x.name.ToLower() == @params[2]).First().hourlyannouncement.ToString();
                                }
                            }
                            Administrative.XMLFileHandler.writeFile(UserList, "Users");
                        }
                        return "";
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
