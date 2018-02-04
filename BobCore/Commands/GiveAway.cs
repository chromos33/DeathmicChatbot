using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BobCore.DataClasses;
using System.Collections.ObjectModel;
using BobCore.Administrative;
using Discord;
using Discord.WebSocket;

namespace BobCore.Commands
{
    class createGiveAway : IFCommand
    {
        private string Trigger = "!creategiveaway";
        private string[] Requirements = { "GiveAway"};
        public string category { get { return "GiveAway"; } }
        public string description { get { return "Erstelle ein GiveAway"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public TrulyObservableCollection<DataClasses.GiveAway> lGiveAway;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if(@params[0].Contains("help"))
                    {
                        return HelpMessage();
                    }
                    else
                    {
                        if(lGiveAway.Where(x => x.Name.ToLower() == @params[0]).Count() > 0)
                        {
                            return "Ein GiveAway mit diesem Namen existiert bereits.";
                        }
                        else
                        {
                            lGiveAway.Add(new GiveAway(@params[0], username));
                            return "GiveAway hinzugefügt.";
                        }
                        
                    }
                }
                else
                {
                    return HelpMessage();
                }
            }
            return "";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.GiveAway")
            {
                lGiveAway = _DataList;
            }
        }
        public string HelpMessage()
        {
            return "!createGiveAway [Name]";
        }
    }
    class addGiveAwayItem : IFCommand
    {
        private string Trigger = "!addgiveawayitem";
        private string[] Requirements = { "User", "GiveAway" };
        public string category { get { return "GiveAway"; } }
        public string description { get { return "Fügt ein Geschenk der Liste Hinzu"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.GiveAway> lGiveAway;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return HelpMessage();
                    }
                    else
                    {
                        if (@params.Count() >= 2)
                        {
                            string output = "";
                            if (lGiveAway.Where(x => x.Name.ToLower() == @params[0]).Count() > 0)
                            {
                                DataClasses.GiveAway GiveAway = lGiveAway.Where(x => x.Name.ToLower() == @params[0]).FirstOrDefault();
                                
                                if (int.TryParse(@params[1], out int SteamID))
                                {
                                    List<List<string>> Multiparameters = Useful_Functions.MultiMessageParameters(message, sTrigger);
                                    foreach(List<string> multiparams in Multiparameters)
                                    {
                                        if(multiparams.Count() == 0)
                                        {
                                            return "Element " + Multiparameters.IndexOf(multiparams) + " und nachfolgende wurden nicht hinzugefügt. Darauf achten das ' , ' vor und nach dem Komma ein Leerzeichen ist";
                                        }
                                        DataClasses.GiveAwayItem newGiveAwayItem = new DataClasses.GiveAwayItem();
                                        if(!int.TryParse(multiparams[0], out int singleSteamID))
                                        {
                                            return multiparams[0] + " ist keine SteamID bitte diesen und nachfolgende korrigieren und nochmals versuchen";
                                        }
                                        newGiveAwayItem.iSteamID = singleSteamID;
                                        newGiveAwayItem.FillDataFromSteam();
                                        if (GiveAway.Items.Count() > 0)
                                        {
                                            newGiveAwayItem.ID = GiveAway.Items.OrderByDescending(x => x.ID).FirstOrDefault().ID + 1;
                                        }
                                        else
                                        {
                                            newGiveAwayItem.ID = 1;
                                        }
                                        if (multiparams.Count() > 1)
                                        {
                                            newGiveAwayItem.sKey = multiparams[1];
                                        }
                                        newGiveAwayItem.sGifter = username;
                                        newGiveAwayItem.current = false;
                                        GiveAway.AddItem(newGiveAwayItem);
                                    }
                                    if(Multiparameters.Count() > 1)
                                    {
                                        output = "Games added to GiveAwayItem pool "+ @params[0];
                                    }
                                    else
                                    {
                                        output = "Game added to GiveAwayItem pool " + @params[0];
                                    }
                                }
                                else
                                {
                                    Uri uriResult;
                                    bool result = Uri.TryCreate(@params[2], UriKind.Absolute, out uriResult)
                                        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                                    if (result)
                                    {
                                        DataClasses.GiveAwayItem newGiveAwayItem = new DataClasses.GiveAwayItem();
                                        if (GiveAway.Items.Count() > 0)
                                        {
                                            newGiveAwayItem.ID = GiveAway.Items.OrderByDescending(x => x.ID).FirstOrDefault().ID + 1;
                                        }
                                        else
                                        {
                                            newGiveAwayItem.ID = 1;
                                        }
                                        output = "Game added to GiveAwayItem pool";
                                        newGiveAwayItem.current = false;

                                        newGiveAwayItem.Link = @params[1];
                                        if (@params.Count == 3)
                                        {
                                            newGiveAwayItem.sKey = @params[3];
                                        }
                                        newGiveAwayItem.sGifter = username;
                                        newGiveAwayItem.current = false;
                                        GiveAway.AddItem(newGiveAwayItem);
                                    }
                                    else
                                    {
                                        return "Link muss wirklich ein Link (z.B. 'http://www.google.de') sein!";
                                    }
                                }
                            }
                            else
                            {
                                output = "Es existiert kein GiveAway mit diesem Namen";
                            }
                            
                            return output;

                        }
                        else
                        {
                            return HelpMessage();
                        }
                    }
                }
            }
            return "";
        }
        public string HelpMessage()
        {
            return Trigger + " [GiveAwayName] \"[Titel]\" [Link] ([CD Key])" + Environment.NewLine + Trigger + " [GiveAwayName] [SteamID] ([CD Key])";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                lUser = _DataList;
            }
            if (type == "DataClasses.GiveAway")
            {
                lGiveAway = _DataList;
            }
        }
    }
    class GiveAwayList : IFCommand
    {
        private string Trigger = "!giveawaylist";
        private string[] Requirements = { "GiveAway" };
        public string category { get { return "GiveAway"; } }
        public string description { get { return "Listet alle GiveAways auf"; } }
        public bool @private { get { return true; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.GiveAway> lGiveAway;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                string output = "";
                foreach(DataClasses.GiveAway item in lGiveAway)
                {
                    output += item.Name + Environment.NewLine;
                }
                return output;
            }
            return "";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.GiveAway")
            {
                lGiveAway = _DataList;
            }
        }
    }
    class GiveAwayItemList : IFCommand
    {
        private string Trigger = "!giveawayitemlist";
        private string[] Requirements = { "User", "GiveAway" };
        public string category { get { return "GiveAway"; } }
        public string description { get { return "Listet alle Geschenke auf die du dem Pool hinzugefügt hast"; } }
        public bool @private { get { return true; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.GiveAway> lGiveAway;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                string output = "";
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return HelpMessage();
                    }
                    else
                    {
                        GiveAway GiveAway = lGiveAway.Where(x => x.Name.ToLower() == @params[0].ToLower()).FirstOrDefault();
                        if (GiveAway != null)
                        {
                            var filteredList = GiveAway.Items.ToList().Where(x => x.sGifter.ToLower() == username.ToLower());
                            foreach (DataClasses.GiveAwayItem GiveAwayItem in filteredList)
                            {
                                output += String.Format("[{0}]: {1} {2}", GiveAwayItem.ID, GiveAwayItem.sTitle, GiveAwayItem.sKey) + System.Environment.NewLine;
                            }
                            return output;
                        }
                    }
                }
            }
            return "";
        }
        public string HelpMessage()
        {
            return Trigger + " [GiveAway Name]";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                lUser = _DataList;
            }
            if (type == "DataClasses.GiveAway")
            {
                lGiveAway = _DataList;
            }
        }
    }
    class RemoveGiveAwayItem : IFCommand
    {
        private string Trigger = "!removegiveawayitem";
        private string[] Requirements = { "User", "GiveAway" };
        public string description { get { return "Entfernt das jeweilige Geschenk aus der Liste (nur selbst hinzugefügte)"; } }
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.GiveAway> lGiveAway;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return HelpMessage();
                    }
                    else
                    {
                        GiveAway GiveAway = lGiveAway.Where(x => x.Name.ToLower() == @params[0].ToLower()).FirstOrDefault();
                        if(GiveAway != null)
                        {
                            int id;
                            if (Int32.TryParse(@params[1], out id))
                            {
                                int index = GiveAway.Items.ToList().FindIndex(x => x.sGifter.ToLower() == username.ToLower() && x.ID == id);
                                if (index >= 0)
                                {
                                    GiveAway.Items.RemoveAt(index);

                                    return "Removed";
                                }
                                else
                                {
                                    return "Geschenk existiert entweder nicht oder wurde nicht von dir eingetragen.";
                                }
                            }
                        }                        
                        return "GiveAway existiert nicht unter diesem Namen";
                    }
                }
            }
            return "";
        }
        public string HelpMessage()
        {
            return Trigger + " [GiveAwayName] [ID] benutze !GiveAwayItemList um ID zu finden";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                lUser = _DataList;
            }
            if (type == "DataClasses.GiveAway")
            {
                lGiveAway = _DataList;
            }
        }
    }
    /*
    class ListMyGifts : IFCommand
    {
        private string Trigger = "!listmygifts";
        private static string[] Requirements = { "User", "GiveAway", "Client" };
        public string description { get { return "Listet alle Geschenke auf die du entweder erhalten hast oder noch weitergeben musst"; } }
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }

        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.GiveAway> lGiveAway;
        public DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return HelpMessage();
                    }
                    else
                    {
                        //Send Instructions for people to give their gifts to persons
                        GiveAway GiveAway = lGiveAway.Where(x => x.Name.ToLower() == @params[0].ToLower()).FirstOrDefault();
                        List<GiveAwayItem> lGiveAwayItem = GiveAway.Items.ToList();
                        if (GiveAway != null)
                        {
                            foreach (var GiveAwayItem in lGiveAwayItem.Where(x => (x.sKey == null || x.sKey == "") && x.sGiftee != "" && (x.sGifter.ToLower() == username.ToLower())).GroupBy(x => x.sGifter).ToList())
                            {
                                try
                                {
                                    string GiveAwayItemmessage = "Verteile folgende Geschenke:" + Environment.NewLine;
                                    foreach (GiveAwayItem item in GiveAwayItem)
                                    {
                                        GiveAwayItemmessage += item.sTitle + " an " + item.sGiftee + Environment.NewLine;
                                    }
                                    // GiveAwayItemmessage = "Du hast" + GiveAwayItem.sTitle + " von " + GiveAwayItem.sGifter + " erhalten. " + GiveAwayItem.sKey;
                                    //Console.WriteLine(GiveAwayItemmessage);
                                    Client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(x => x.Username.ToLower() == GiveAwayItem.Key.ToLower()).FirstOrDefault().SendMessageAsync(GiveAwayItemmessage);
                                }
                                catch (NullReferenceException)
                                {

                                }
                            }
                            foreach (var GiveAwayItem in lGiveAwayItem.Where(x => x.sGiftee.ToLower() == username.ToLower()).GroupBy(x => x.sGiftee).ToList())
                            {
                                try
                                {
                                    string GiveAwayItemmessage = "Du erhältst folgende Geschenke:" + Environment.NewLine;
                                    foreach (GiveAwayItem item in GiveAwayItem)
                                    {
                                        GiveAwayItemmessage += item.sTitle + " von " + item.sGifter + " : " + item.sKey + Environment.NewLine;
                                    }
                                    // GiveAwayItemmessage = "Du hast" + GiveAwayItem.sTitle + " von " + GiveAwayItem.sGifter + " erhalten. " + GiveAwayItem.sKey;
                                    //Console.WriteLine(GiveAwayItemmessage);
                                    Client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(x => x.Username.ToLower() == GiveAwayItem.Key.ToLower()).FirstOrDefault().SendMessageAsync(GiveAwayItemmessage);
                                }
                                catch (NullReferenceException)
                                {

                                }
                            }
                        }
                    }
                }
            }
            return "";
        }
        public string HelpMessage()
        {
            return Trigger + " [GiveAwayName]";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                lUser = _DataList;
            }
            if (type == "DataClasses.GiveAway")
            {
                lGiveAway = _DataList;
            }
            if (type == "Client")
            {
                Client = _DataList;
            }
        }
    }
    /*
    class GiveGiveAwayItem : IFCommand
    {
        private string Trigger = "!changeGiveAwayItem";
        private string[] Requirements = { "User", "GiveAwayItem", "Client" };
        public string description { get { return "Um ein Geschenk das du gewonnen hast jemandem anderen zuzuweisen"; } }
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.GiveAwayItem> lGiveAwayItem;
        public DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return Trigger + " [id] [username] benutze !GiveAwayItemlist um ID zu finden";
                    }
                    else
                    {
                        if (@params.Count() == 2)
                        {
                            int id;
                            if (Int32.TryParse(@params[0], out id))
                            {
                                int index = lGiveAwayItem.ToList().FindIndex(x => x.sGiftee.ToLower() == username.ToLower() && x.ID == id);
                                if (index >= 0)
                                {
                                    Console.WriteLine(@params[1]);
                                    var user = Client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(x => x.Username.ToLower() == @params[1].ToLower() || x.Nickname != null && x.Nickname.ToLower() == @params[1].ToLower()).FirstOrDefault();
                                    if (user != null)
                                    {
                                        lGiveAwayItem[index].setGiftee(@params[1].ToLower());

                                        user.SendMessageAsync(username + " hat dir " + lGiveAwayItem[index].sTitle + "geschickt");
                                        return "Geschenk umverteilt";
                                    }
                                }
                                else
                                {
                                    return "Geschenk existiert entweder nicht oder wurde dir nicht zugewiesen.";
                                }
                            }
                        }
                        return "error";
                    }
                }
            }
            return "";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                lUser = _DataList;
            }
            if (type == "DataClasses.GiveAwayItem")
            {
                lGiveAwayItem = _DataList;
            }
            if (type == "Client")
            {
                Client = _DataList;
            }
        }
    }
    class NextGiveAway : IFCommand
    {
        private string Trigger = "!nextchristmasgiveaway";
        private static string[] Requirements = { "User", "GiveAwayItem", "Client" };
        public string description { get { return "Das nächste Geschenk aus der Liste zum verlosen zu wählen [RESTRICTED]"; } }
        public List<string> UserRestriction = new List<string> { "chromos33", "deathmic", "vampi" };
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.GiveAwayItem> lGiveAwayItem;
        public DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                if (UserRestriction.Contains(username.ToLower()))
                {
                    //TODO: Check for Chrismasdate (Range)
                    var disablegiveaway = lGiveAwayItem.Where(x => x.current).FirstOrDefault();
                    if (disablegiveaway != null)
                    {
                        disablegiveaway.current = false;

                    }
                    var giveaway = lGiveAwayItem.Where(x => x.sGiftee == null || x.sGiftee != null && x.sGiftee == "").OrderBy(x => Guid.NewGuid()).Take(1);
                    if (giveaway.Count() > 0)
                    {
                        var test = giveaway.FirstOrDefault();
                        test.current = true;
                        BobCore.Administrative.XMLFileHandler.writeFile(lGiveAwayItem, "GiveAwayList");
                        return $"Nächstes Giveaway: {test.sTitle} at {test.Link} benutze !applytoroulette um teilzunehmen";
                    }
                    else
                    {
                        return "Keine Geschenke mehr.";
                    }
                }
                else
                {
                    return "Forbidden";
                }

            }
            return "";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                lUser = _DataList;
            }
            if (type == "DataClasses.GiveAwayItem")
            {
                lGiveAwayItem = _DataList;
            }
            if (type == "Client")
            {
                Client = _DataList;
            }
        }
    }
    class GiveAwayItemRecipient
    {
        public string recipient;
        public int count;
        public GiveAwayItemRecipient()
        {

        }
        public GiveAwayItemRecipient(string _name, int _count)
        {
            recipient = _name;
            count = _count;
        }
    }
    class GiveAwayRoulette : IFCommand
    {
        private string Trigger = "!giveawayroulette";
        private static string[] Requirements = { "User", "GiveAwayItem", "Client" };
        public List<string> UserRestriction = new List<string> { "chromos33", "deathmic", "vampi" };
        public string description { get { return "Verlost das momentan ausgewählte Spiel unter den Teilnehmern [RESTRICTED]"; } }
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.GiveAwayItem> lGiveAwayItem;
        public DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                if (UserRestriction.Contains(username.ToLower()))
                {
                    var giveaway = lGiveAwayItem.Where(x => x.current);
                    string @return = "";
                    if (giveaway.Count() > 0)
                    {

                        var gift = giveaway.FirstOrDefault();
                        if (gift.Applicants.Count() > 0)
                        {

                            List<GiveAwayItemRecipient> GiveAwayItemRecipient = new List<GiveAwayItemRecipient>();
                            foreach (string applicant in gift.Applicants)
                            {
                                GiveAwayItemRecipient.Add(new GiveAwayItemRecipient(applicant, lGiveAwayItem.Where(x => x.sGiftee != null && x.sGiftee.ToLower() == applicant.ToLower()).Count()));
                            }
                            var users = GiveAwayItemRecipient.GroupBy(x => x.count).ToList().OrderBy(x => x.Key).FirstOrDefault();
                            if (users != null)
                            {
                                var randomUser = users.OrderBy(x => Guid.NewGuid()).Take(1).FirstOrDefault();
                                if (randomUser != null)
                                {
                                    gift.setGiftee(randomUser.recipient);
                                    gift.current = false;
                                    gift.removeApplicant(gift.sGiftee);
                                }
                            }
                            if (gift.Applicants.Count() > 0 && lGiveAwayItem.Where(x => x.sTitle.ToLower() == gift.sTitle.ToLower() && x.sGiftee == "").Count() > 0)
                            {
                                @return = "Und gewonnen haben:" + Environment.NewLine;
                                @return += gift.sGiftee + Environment.NewLine;
                                var identicalGiveAwayItems = lGiveAwayItem.Where(x => x.sTitle.ToLower() == gift.sTitle.ToLower() && x.sGiftee == "");
                                foreach (var item in identicalGiveAwayItems)
                                {
                                    if (gift.Applicants.Count() > 0)
                                    {
                                        GiveAwayItemRecipient = new List<GiveAwayItemRecipient>();
                                        foreach (string applicant in gift.Applicants)
                                        {
                                            GiveAwayItemRecipient.Add(new GiveAwayItemRecipient(applicant, lGiveAwayItem.Where(x => x.sGiftee != null && x.sGiftee.ToLower() == applicant.ToLower()).Count()));
                                        }
                                        users = GiveAwayItemRecipient.GroupBy(x => x.count).ToList().OrderBy(x => x.Key).FirstOrDefault();
                                        if (users != null)
                                        {
                                            var randomUser = users.OrderBy(x => Guid.NewGuid()).Take(1).FirstOrDefault();
                                            if (randomUser != null)
                                            {
                                                item.sGiftee = randomUser.recipient;
                                                item.current = false;
                                                @return += randomUser.recipient + Environment.NewLine;
                                                gift.removeApplicant(item.sGiftee);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                @return = "Und gewonnen hat: " + gift.sGiftee;
                            }
                            //var winner = gift.Applicants.OrderBy(x => Guid.NewGuid()).Take(1);


                            // TODO: check if there are more gifts with this title and if yes assign other participants the remaining copies

                            return @return;
                        }
                        else
                        {
                            return "Keine Teilnehmer";
                        }




                    }
                }
                else
                {
                    return "Forbidden";
                }

            }
            return "";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                lUser = _DataList;
            }
            if (type == "DataClasses.GiveAwayItem")
            {
                lGiveAwayItem = _DataList;
            }
            if (type == "Client")
            {
                Client = _DataList;
            }
        }
    }
    class ApplyToGiveAwayRoulette : IFCommand
    {
        private string Trigger = "!applytoroulette";
        private static string[] Requirements = { "User", "GiveAwayItem", "Client" };
        public string description { get { return "Hiermit nimmst du an der momentanen Verlosung teil"; } }
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return true; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.GiveAwayItem> lGiveAwayItem;
        public DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            try
            {

                if (message.ToLower().Contains(sTrigger))
                {
                    var giveaway = lGiveAwayItem.Where(x => x.current);
                    if (giveaway.Count() > 0)
                    {
                        var gift = giveaway.FirstOrDefault();
                        //TODO change from simple random to also consider how many things 
                        if (gift.Applicants.Contains(username))
                        {
                            return "Nimmst bereits Teil";
                        }
                        else
                        {
                            gift.addApplicant(username);
                            //gift.Applicants.Add(username);

                            return "Teilnamebestätigung" + Environment.NewLine + "mit !unapplyfromroulette könnt ihr die Teilnahme zurückziehen";
                        }

                    }
                    else
                    {
                        return "Kein aktives Giveaway";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
            return "";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                lUser = _DataList;
            }
            if (type == "DataClasses.GiveAwayItem")
            {
                lGiveAwayItem = _DataList;
            }
            if (type == "Client")
            {
                Client = _DataList;
            }
        }
    }
    class UnApplyToGiveAwayRoulette : IFCommand
    {
        private string Trigger = "!unapplyfromroulette";
        private static string[] Requirements = { "User", "GiveAwayItem", "Client" };
        public string description { get { return "Hiermit nimmst du deine Teilnahme an einer Verlosung zurück"; } }
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return true; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.GiveAwayItem> lGiveAwayItem;
        public DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                var giveaway = lGiveAwayItem.Where(x => x.current);
                if (giveaway.Count() > 0)
                {
                    var gift = giveaway.FirstOrDefault();
                    //TODO change from simple random to also consider how many things 
                    if (gift.Applicants.Contains(username))
                    {
                        gift.removeApplicant(username);

                        return "Teilnahme zurückgenommen";
                    }
                    else
                    {
                        return "Nimmst gar nicht Teil";

                    }
                }
                return "Gerade kein Geschenk zur verlosung";
            }
            return "";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                lUser = _DataList;
            }
            if (type == "DataClasses.GiveAwayItem")
            {
                lGiveAwayItem = _DataList;
            }
            if (type == "Client")
            {
                Client = _DataList;
            }
        }
    }
    class FinalizeGiveAway : IFCommand
    {
        private string Trigger = "!sendgiftnotifications";
        private static string[] Requirements = { "User", "GiveAwayItem", "Client" };
        public string description { get { return "Hiermit werden alle bisher verteilten Geschenke entweder direkt weiter geleitet oder die Person angeschrieben die das Geschenk dann vergeben muss [RESTRICTED]"; } }
        public List<string> UserRestriction = new List<string> { "chromos33", "deathmic", "vampi" };
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }

        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.GiveAwayItem> lGiveAwayItem;
        public DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                if (UserRestriction.Contains(username.ToLower()))
                {
                    //Send Instructions for people to give their gifts to persons
                    foreach (var GiveAwayItem in lGiveAwayItem.Where(x => (x.sKey == null || x.sKey == "") && x.sGiftee != "").GroupBy(x => x.sGifter).ToList())
                    {
                        try
                        {
                            string GiveAwayItemmessage = "Verteile folgende Geschenke:" + Environment.NewLine;
                            foreach (GiveAwayItem item in GiveAwayItem)
                            {
                                GiveAwayItemmessage += item.sTitle + " an " + item.sGiftee + Environment.NewLine;
                            }
                            // GiveAwayItemmessage = "Du hast" + GiveAwayItem.sTitle + " von " + GiveAwayItem.sGifter + " erhalten. " + GiveAwayItem.sKey;
                            Client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(x => x.Username.ToLower() == GiveAwayItem.Key.ToLower()).FirstOrDefault().SendMessageAsync(GiveAwayItemmessage);
                        }
                        catch (NullReferenceException)
                        {
                            Console.WriteLine("test");
                        }
                    }
                    foreach (var GiveAwayItem in lGiveAwayItem.Where(x => (x.sKey != null && x.sKey != "") && x.sGiftee != "").GroupBy(x => x.sGiftee).ToList())
                    {
                        try
                        {
                            string GiveAwayItemmessage = "Du erhältst folgende Geschenke:" + Environment.NewLine;
                            foreach (GiveAwayItem item in GiveAwayItem)
                            {
                                GiveAwayItemmessage += item.sTitle + " von " + item.sGifter + " : " + item.sKey + Environment.NewLine;
                            }
                            // GiveAwayItemmessage = "Du hast" + GiveAwayItem.sTitle + " von " + GiveAwayItem.sGifter + " erhalten. " + GiveAwayItem.sKey;
                            Client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(x => x.Username.ToLower() == GiveAwayItem.Key.ToLower()).FirstOrDefault().SendMessageAsync(GiveAwayItemmessage);
                        }
                        catch (NullReferenceException)
                        {
                            Console.WriteLine("test");
                        }
                    }

                }
                else
                {
                    return "Forbidden";
                }

            }
            return "";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                lUser = _DataList;
            }
            if (type == "DataClasses.GiveAwayItem")
            {
                lGiveAwayItem = _DataList;
            }
            if (type == "Client")
            {
                Client = _DataList;
            }
        }
    }
    class RemainingGiveAwayItems : IFCommand
    {
        private string Trigger = "!remainingGiveAwayItems";
        private static string[] Requirements = { "GiveAwayItem" };
        public string description { get { return "Gibt die Anzahl an Geschenken zurück"; } }
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public TrulyObservableCollection<DataClasses.GiveAwayItem> lGiveAwayItem;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            try
            {
                if (message.ToLower().Contains(sTrigger))
                {
                    if (lGiveAwayItem != null)
                    {
                        return lGiveAwayItem.Where(x => x.sGiftee == "").Count().ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
            return "";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.GiveAwayItem")
            {
                lGiveAwayItem = _DataList;
            }
        }
    }
    */
}
