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
    class addPresent : IFCommand
    {
        private string Trigger = "!addpresent";
        private string[] Requirements = { "User", "Present", "FileUpdater" };
        public string category { get { return "GiveAway"; } }
        public string description { get { return "Fügt ein Geschenk der Liste Hinzu"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.Present> lPresent;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return Trigger + " \"[Titel]\" [Link] ([CD Key])";
                    }
                    else
                    {
                        if (@params.Count() >= 2)
                        {
                            Uri uriResult;
                            bool result = Uri.TryCreate(@params[1], UriKind.Absolute, out uriResult)
                                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                            if (result)
                            {
                                string output = "";
                                DataClasses.Present newPresent = new DataClasses.Present();

                                if (lPresent.Count() > 0)
                                {
                                    newPresent.ID = lPresent.OrderByDescending(x => x.ID).FirstOrDefault().ID + 1;
                                }
                                else
                                {
                                    newPresent.ID = 1;
                                }

                                newPresent.sTitle = @params[0];
                                newPresent.sGifter = username;
                                newPresent.current = false;
                                output = "Game added to present pool";
                                newPresent.Link = @params[1];
                                if (@params.Count == 3)
                                {
                                    newPresent.sKey = @params[2];
                                }
                                lPresent.Add(newPresent);

                                if (output == "")
                                {
                                    return Trigger + " \"[Titel]\" [Link] ([CD Key])";
                                }
                                return output;
                            }
                            else
                            {
                                return "Link muss wirklich ein Link (z.B. 'http://www.google.de') sein!";
                            }
                        }
                        else
                        {
                            return Trigger + " \"[Titel]\" [Link] ([CD Key])";
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
                lUser = _DataList;
            }
            if (type == "DataClasses.Present")
            {
                lPresent = _DataList;
            }
        }
    }
    class PresentList : IFCommand
    {
        private string Trigger = "!presentlist";
        private string[] Requirements = { "User", "Present" };
        public string category { get { return "GiveAway"; } }
        public string description { get { return "Listet alle Geschenke auf die du dem Pool hinzugefügt hast"; } }
        public bool @private { get { return true; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.Present> lPresent;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                string output = "";
                var filteredList = lPresent.Where(x => x.sGifter.ToLower() == username.ToLower());
                foreach (DataClasses.Present present in filteredList)
                {
                    output += String.Format("[{0}]: {1} {2}", present.ID, present.sTitle, present.sKey) + System.Environment.NewLine;
                }
                return output;
            }
            return "";
        }
        public void addRequiredList(dynamic _DataList, string type)
        {
            if (type == "DataClasses.User")
            {
                lUser = _DataList;
            }
            if (type == "DataClasses.Present")
            {
                lPresent = _DataList;
            }
        }
    }
    class RemovePresent : IFCommand
    {
        private string Trigger = "!removepresent";
        private string[] Requirements = { "User", "Present" };
        public string description { get { return "Entfernt das jeweilige Geschenk aus der Liste (nur selbst hinzugefügte)"; } }
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.Present> lPresent;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                List<string> @params = Useful_Functions.MessageParameters(message, sTrigger);
                if (@params.Count() > 0)
                {
                    if (@params[0] == "help")
                    {
                        return Trigger + " [id] benutze !presentlist um ID zu finden";
                    }
                    else
                    {
                        int id;
                        if (Int32.TryParse(@params[0], out id))
                        {
                            int index = lPresent.ToList().FindIndex(x => x.sGifter.ToLower() == username.ToLower() && x.ID == id);
                            if (index >= 0)
                            {
                                lPresent.RemoveAt(index);

                                return "Removed";
                            }
                            else
                            {
                                return "Geschenk existiert entweder nicht oder wurde nicht von dir eingetragen.";
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
            if (type == "DataClasses.Present")
            {
                lPresent = _DataList;
            }
        }
    }
    class GivePresent : IFCommand
    {
        private string Trigger = "!changepresent";
        private string[] Requirements = { "User", "Present", "Client" };
        public string description { get { return "Um ein Geschenk das du gewonnen hast jemandem anderen zuzuweisen"; } }
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.Present> lPresent;
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
                        return Trigger + " [id] [username] benutze !presentlist um ID zu finden";
                    }
                    else
                    {
                        if (@params.Count() == 2)
                        {
                            int id;
                            if (Int32.TryParse(@params[0], out id))
                            {
                                int index = lPresent.ToList().FindIndex(x => x.sGiftee.ToLower() == username.ToLower() && x.ID == id);
                                if (index >= 0)
                                {
                                    Console.WriteLine(@params[1]);
                                    var user = Client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(x => x.Username.ToLower() == @params[1].ToLower() || x.Nickname != null && x.Nickname.ToLower() == @params[1].ToLower()).FirstOrDefault();
                                    if (user != null)
                                    {
                                        lPresent[index].setGiftee(@params[1].ToLower());

                                        user.SendMessageAsync(username + " hat dir " + lPresent[index].sTitle + "geschickt");
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
            if (type == "DataClasses.Present")
            {
                lPresent = _DataList;
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
        private static string[] Requirements = { "User", "Present", "Client" };
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
        public TrulyObservableCollection<DataClasses.Present> lPresent;
        public DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                if (UserRestriction.Contains(username.ToLower()))
                {
                    //TODO: Check for Chrismasdate (Range)
                    var disablegiveaway = lPresent.Where(x => x.current).FirstOrDefault();
                    if (disablegiveaway != null)
                    {
                        disablegiveaway.current = false;

                    }
                    var giveaway = lPresent.Where(x => x.sGiftee == null || x.sGiftee != null && x.sGiftee == "").OrderBy(x => Guid.NewGuid()).Take(1);
                    if (giveaway.Count() > 0)
                    {
                        var test = giveaway.FirstOrDefault();
                        test.current = true;
                        BobCore.Administrative.XMLFileHandler.writeFile(lPresent, "GiveAwayList");
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
            if (type == "DataClasses.Present")
            {
                lPresent = _DataList;
            }
            if (type == "Client")
            {
                Client = _DataList;
            }
        }
    }
    class PresentRecipient
    {
        public string recipient;
        public int count;
        public PresentRecipient()
        {

        }
        public PresentRecipient(string _name, int _count)
        {
            recipient = _name;
            count = _count;
        }
    }
    class GiveAwayRoulette : IFCommand
    {
        private string Trigger = "!giveawayroulette";
        private static string[] Requirements = { "User", "Present", "Client" };
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
        public TrulyObservableCollection<DataClasses.Present> lPresent;
        public DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                if (UserRestriction.Contains(username.ToLower()))
                {
                    var giveaway = lPresent.Where(x => x.current);
                    string @return = "";
                    if (giveaway.Count() > 0)
                    {

                        var gift = giveaway.FirstOrDefault();
                        if (gift.Applicants.Count() > 0)
                        {

                            List<PresentRecipient> presentRecipient = new List<PresentRecipient>();
                            foreach (string applicant in gift.Applicants)
                            {
                                presentRecipient.Add(new PresentRecipient(applicant, lPresent.Where(x => x.sGiftee != null && x.sGiftee.ToLower() == applicant.ToLower()).Count()));
                            }
                            var users = presentRecipient.GroupBy(x => x.count).ToList().OrderBy(x => x.Key).FirstOrDefault();
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
                            if (gift.Applicants.Count() > 0 && lPresent.Where(x => x.sTitle.ToLower() == gift.sTitle.ToLower() && x.sGiftee == "").Count() > 0)
                            {
                                @return = "Und gewonnen haben:" + Environment.NewLine;
                                @return += gift.sGiftee + Environment.NewLine;
                                var identicalpresents = lPresent.Where(x => x.sTitle.ToLower() == gift.sTitle.ToLower() && x.sGiftee == "");
                                foreach (var item in identicalpresents)
                                {
                                    if (gift.Applicants.Count() > 0)
                                    {
                                        presentRecipient = new List<PresentRecipient>();
                                        foreach (string applicant in gift.Applicants)
                                        {
                                            presentRecipient.Add(new PresentRecipient(applicant, lPresent.Where(x => x.sGiftee != null && x.sGiftee.ToLower() == applicant.ToLower()).Count()));
                                        }
                                        users = presentRecipient.GroupBy(x => x.count).ToList().OrderBy(x => x.Key).FirstOrDefault();
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
            if (type == "DataClasses.Present")
            {
                lPresent = _DataList;
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
        private static string[] Requirements = { "User", "Present", "Client" };
        public string description { get { return "Hiermit nimmst du an der momentanen Verlosung teil"; } }
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return true; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.Present> lPresent;
        public DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            try
            {

                if (message.ToLower().Contains(sTrigger))
                {
                    var giveaway = lPresent.Where(x => x.current);
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
            if (type == "DataClasses.Present")
            {
                lPresent = _DataList;
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
        private static string[] Requirements = { "User", "Present", "Client" };
        public string description { get { return "Hiermit nimmst du deine Teilnahme an einer Verlosung zurück"; } }
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return true; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.Present> lPresent;
        public DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                var giveaway = lPresent.Where(x => x.current);
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
            if (type == "DataClasses.Present")
            {
                lPresent = _DataList;
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
        private static string[] Requirements = { "User", "Present", "Client" };
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
        public TrulyObservableCollection<DataClasses.Present> lPresent;
        public DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                if (UserRestriction.Contains(username.ToLower()))
                {
                    //Send Instructions for people to give their gifts to persons
                    foreach (var present in lPresent.Where(x => (x.sKey == null || x.sKey == "") && x.sGiftee != "").GroupBy(x => x.sGifter).ToList())
                    {
                        try
                        {
                            string presentmessage = "Verteile folgende Geschenke:" + Environment.NewLine;
                            foreach (Present item in present)
                            {
                                presentmessage += item.sTitle + " an " + item.sGiftee + Environment.NewLine;
                            }
                            // presentmessage = "Du hast" + present.sTitle + " von " + present.sGifter + " erhalten. " + present.sKey;
                            Client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(x => x.Username.ToLower() == present.Key.ToLower()).FirstOrDefault().SendMessageAsync(presentmessage);
                        }
                        catch (NullReferenceException)
                        {

                        }
                    }
                    foreach (var present in lPresent.Where(x => (x.sKey != null && x.sKey != "") && x.sGiftee != "").GroupBy(x => x.sGiftee).ToList())
                    {
                        try
                        {
                            string presentmessage = "Du erhältst folgende Geschenke:" + Environment.NewLine;
                            foreach (Present item in present)
                            {
                                presentmessage += item.sTitle + " von " + item.sGifter + " : " + item.sKey + Environment.NewLine;
                            }
                            // presentmessage = "Du hast" + present.sTitle + " von " + present.sGifter + " erhalten. " + present.sKey;
                            Client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(x => x.Username.ToLower() == present.Key.ToLower()).FirstOrDefault().SendMessageAsync(presentmessage);
                        }
                        catch (NullReferenceException)
                        {

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
            if (type == "DataClasses.Present")
            {
                lPresent = _DataList;
            }
            if (type == "Client")
            {
                Client = _DataList;
            }
        }
    }
    class ListMyGifts : IFCommand
    {
        private string Trigger = "!listmygifts";
        private static string[] Requirements = { "User", "Present", "Client" };
        public string description { get { return "Listet alle Geschenke auf die du entweder erhalten hast oder noch weitergeben musst"; } }
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }

        public List<DataClasses.User> lUser;
        public TrulyObservableCollection<DataClasses.Present> lPresent;
        public DiscordSocketClient Client;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            if (message.ToLower().Contains(sTrigger))
            {
                //Send Instructions for people to give their gifts to persons
                foreach (var present in lPresent.Where(x => (x.sKey == null || x.sKey == "") && x.sGiftee != "" && (x.sGifter == username.ToLower())).GroupBy(x => x.sGifter).ToList())
                {
                    try
                    {
                        string presentmessage = "Verteile folgende Geschenke:" + Environment.NewLine;
                        foreach (Present item in present)
                        {
                            presentmessage += item.sTitle + " an " + item.sGiftee + Environment.NewLine;
                        }
                        // presentmessage = "Du hast" + present.sTitle + " von " + present.sGifter + " erhalten. " + present.sKey;
                        Client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(x => x.Username.ToLower() == present.Key.ToLower()).FirstOrDefault().SendMessageAsync(presentmessage);
                    }
                    catch (NullReferenceException)
                    {

                    }
                }
                foreach (var present in lPresent.Where(x => x.sGiftee == username).GroupBy(x => x.sGiftee).ToList())
                {
                    try
                    {
                        string presentmessage = "Du erhältst folgende Geschenke:" + Environment.NewLine;
                        foreach (Present item in present)
                        {
                            presentmessage += item.sTitle + " von " + item.sGifter + " : " + item.sKey + Environment.NewLine;
                        }
                        // presentmessage = "Du hast" + present.sTitle + " von " + present.sGifter + " erhalten. " + present.sKey;
                        Client.Guilds.Where(x => x.Name.ToLower() == "deathmic").FirstOrDefault().Users.Where(x => x.Username.ToLower() == present.Key.ToLower()).FirstOrDefault().SendMessageAsync(presentmessage);
                    }
                    catch (NullReferenceException)
                    {

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
            if (type == "DataClasses.Present")
            {
                lPresent = _DataList;
            }
            if (type == "Client")
            {
                Client = _DataList;
            }
        }
    }
    class RemainingPresents : IFCommand
    {
        private string Trigger = "!remainingpresents";
        private static string[] Requirements = { "Present" };
        public string description { get { return "Gibt die Anzahl an Geschenken zurück"; } }
        public string category { get { return "GiveAway"; } }
        public bool @private { get { return false; } }
        public string[] SARequirements
        {
            get { return Requirements; }
        }
        public string sTrigger { get { return Trigger; } }
        public TrulyObservableCollection<DataClasses.Present> lPresent;
        public string CheckCommandAndExecuteIfApplicable(string message, string username, string channel)
        {
            try
            {
                if (message.ToLower().Contains(sTrigger))
                {
                    if (lPresent != null)
                    {
                        return lPresent.Where(x => x.sGiftee == "").Count().ToString();
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
            if (type == "DataClasses.Present")
            {
                lPresent = _DataList;
            }
        }
    }
}
