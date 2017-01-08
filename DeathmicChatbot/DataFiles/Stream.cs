using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DeathmicChatbot.DataFiles
{
    public class Stream
    {
        public string name;
        public bool subscribed;

        public Stream(string _name,bool _subscribed)
        {
            name = _name;
            subscribed = _subscribed;
        }
        public Stream()
        {

        }
    }
    public class internalStream
    {
        public string sChannel;
        public DateTime dtStarttime;
        public DateTime dtStoptime;
        public bool bRunning;
        public string sProvider;
        public string sGame;
        public string sUrl;
        public DateTime dtLastglobalnotice;
        public string sTwitchchat;
        public bool bTwoway;
        public string sTargetrelaychannel;
        public internalStream()
        {

        }
        public internalStream(string _channel,string _sTwitchchat,bool _bTwoway, string _sTargetrelaychannel)
        {
            sChannel = _channel;
            sTwitchchat = _sTwitchchat;
            bTwoway = _bTwoway;
            sTargetrelaychannel = _sTargetrelaychannel;
        }
        public internalStream(string _channel)
        {
            sChannel = _channel;
        }

    }
}
