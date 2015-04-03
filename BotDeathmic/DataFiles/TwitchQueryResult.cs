using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeathmicChatbot.StreamInfo.Twitch;
using DeathmicChatbot.StreamInfo;

namespace DeathmicChatbot.DataFiles
{
    public class TwitchQueryResult
    {
        public TwitchProvider resultprovider { get; set; }
        public StreamEventArgs resultargs { get; set; }
        public TwitchQueryResult(TwitchProvider _resultprovider,StreamEventArgs _resultargs)
        {
            resultprovider = _resultprovider;
            resultargs = _resultargs;
            
        }
    }
}
