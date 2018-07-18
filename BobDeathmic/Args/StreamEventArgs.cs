using System;

namespace BobDeathmic.Args
{
    public class StreamEventArgs
    {
        public string stream;
        public string game;
        //state 1:started;2:running;3:stopped;
        //TODO: Change state to Database.Classes.StreamState
        public Models.Enum.StreamState state;
        public string link;
        public string channel;
        public string Notification;
        public Models.Enum.RelayState relayactive;
        public Models.Enum.StreamProviderTypes StreamType;
        public bool PostUpTime;
        public TimeSpan Uptime;
    }
}
