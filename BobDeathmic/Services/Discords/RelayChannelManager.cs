using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BobDeathmic.Services.Discords
{
    public class RelayChannelManager
    {
        private List<string> ChannelList;
        private string[] _randomRelayChannels;

        public int ListCount { 
            get {
                return ChannelList.Count;
            } 
        }

        public RelayChannelManager(List<string> list) : this(list, new string[0])
        {
        }

        public RelayChannelManager(List<string> list, string[] randomRelayChannels) 
        {
            this.ChannelList = list;
            this._randomRelayChannels = randomRelayChannels;
        }

        public bool AddChannel(string channelname)
        {
            if(InList(channelname) || !Regex.Match(channelname.ToLower(), @"stream_").Success)
            {
                return false;
            }
            ChannelList.Add(channelname);
            return true;
        }
        private bool InList(string channelname)
        {
            return ChannelList.Contains(channelname.ToLower());
        }
        public bool RemoveChannel(string channelname)
        {
            if (!InList(channelname))
            {
                return false;
            }
            ChannelList.Remove(channelname);
            return true;
        }
    }
}
