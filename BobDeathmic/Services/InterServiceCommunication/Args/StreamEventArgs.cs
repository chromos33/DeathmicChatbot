using BobDeathmic.Data.Enums.Relay;
using BobDeathmic.Data.Enums.Stream;
using System;

namespace BobDeathmic.Args
{
    public class StreamEventArgs
    {
        public string stream;
        public string game;
        //state 1:started;2:running;3:stopped;
        //TODO: Change state to Database.Classes.StreamState
        public StreamState state;
        public string link;
        public string Notification;
        public RelayState relayactive;
        public StreamProviderTypes StreamType;
        public bool PostUpTime;
        public TimeSpan Uptime;
    }
}
