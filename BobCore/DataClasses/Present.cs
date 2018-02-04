using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BobCore.DataClasses
{
    public class GiveAwayItem : INotifyPropertyChanged
    {
        public int ID;
        public string sTitle;
        public string sKey;
        public int iSteamID;
        // Gifter is the Person that added this GiveAwayItem to the pool
        public string sGifter;
        // Giftee is the Person receiving this GiveAwayItem
        public string sGiftee;
        public string Link;
        public bool current;
        public List<String> Applicants;
        public void setGiftee(string giftee)
        {
            sGiftee = giftee;
            NotifyPropertyChanged("Applicants");
        }
        public void addApplicant(string applicant)
        {
            Applicants.Add(applicant);
            NotifyPropertyChanged("Applicants");
        }
        public void removeApplicant(string applicant)
        {
            Applicants.Remove(applicant);
            NotifyPropertyChanged("Applicants");
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        public GiveAwayItem()
        {
            if (sGiftee == null)
            {
                sGiftee = "";
            }
        }
        public GiveAwayItem(string _sTitle, string _sGifter, string _sKey = "")
        {
            sTitle = _sTitle;
            sGifter = _sGifter;
            sGiftee = "";
            sKey = _sKey;
            current = false;
        }
        public void SetGiftee(string nick)
        {
            sGiftee = nick;
        }
        public void FillDataFromSteam()
        {
            ServicePointManager.DefaultConnectionLimit = 20;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;
            string url = "http://store.steampowered.com/app/" + iSteamID;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Proxy = null;
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            if (resp.StatusCode == HttpStatusCode.OK)
            {
                string data = "";
                using (var reader = new System.IO.StreamReader(resp.GetResponseStream()))
                {
                    data = reader.ReadToEnd();
                }
                var doc = new HtmlDocument();
                doc.LoadHtml(data);
                var nodes = doc.DocumentNode.Descendants("div").Where(x => x.Attributes["class"] != null && x.Attributes["class"].Value.Contains("apphub_AppName")).FirstOrDefault();
                if(nodes != null)
                {
                    Link = url;
                    sTitle = nodes.InnerHtml;
                }
            }
        }
    }
}
