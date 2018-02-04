using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;
using BobCore.Administrative;
using System.Collections.Specialized;

namespace BobCore.DataClasses
{
    public class GiveAway : INotifyPropertyChanged
    {
        public TrulyObservableCollection<GiveAwayItem> Items = new TrulyObservableCollection<GiveAwayItem>();
        public string Name { get; set; }
        public List<string> Admins { get; set; }
        public GiveAway()
        {
            Items.CollectionChanged += PassCollectionChangedThrough;
        }

        private void PassCollectionChangedThrough(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("Items");
        }

        public GiveAway(string sName,string sUser)
        {
            Name = sName;
            if(Admins == null)
            {
                Admins = new List<string>();
            }
            Admins.Add(sUser);
        }
        public void AddItem(GiveAwayItem _item)
        {
            Items.Add(_item);
        }
        public void RemoveItem(int _index)
        {
            if(Items.Where(x => x.ID == _index).Count() > 0)
            {
                Items.RemoveAt(_index);
            }
        }
        public bool ItemIsFromUSer(int _index, string _user)
        {
            if(Items[_index].sGifter.ToLower() == _user.ToLower())
            {
                return true;
            }
            return false;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
}
