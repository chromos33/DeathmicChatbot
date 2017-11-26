namespace BobCore.StreamFunctions
{
    class StreamEventArgs
    {
        public string stream;
        public string game;
        //state 1:started;2:running;3:stopped;
        public int state;
        public string link;
        public string channel;

    }
}
