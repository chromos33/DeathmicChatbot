using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BobCore.DataClasses
{
    public class Present : INotifyPropertyChanged
    {
        public int ID;
        public string sTitle;
        public string sKey;
        // Gifter is the Person that added this Present to the pool
        public string sGifter;
        // Giftee is the Person receiving this Present
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
        public Present()
        {
            if (sGiftee == null)
            {
                sGiftee = "";
            }
        }
        public Present(string _sTitle, string _sGifter, string _sKey = "")
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
    }
}
