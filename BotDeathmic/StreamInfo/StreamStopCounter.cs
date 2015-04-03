using System.Collections.Generic;

namespace DeathmicChatbot.StreamInfo
{
    internal class StreamStopCounter
    {
        private const int MAXIMUM_NUMBER_OF_TRIES_UNTIL_STREAM_REALLY_STOPS = 3;

        private readonly Dictionary<string, int> _streamStopTries =
            new Dictionary<string, int>();

        public void StreamTriesToStop(string sStreamName)
        {
            if (!_streamStopTries.ContainsKey(sStreamName))
                _streamStopTries.Add(sStreamName, 0);
            _streamStopTries[sStreamName] = _streamStopTries[sStreamName] + 1;
        }

        public bool StreamHasTriedStoppingEnoughTimes(string sStreamName)
        {
            return _streamStopTries[sStreamName] >
                   MAXIMUM_NUMBER_OF_TRIES_UNTIL_STREAM_REALLY_STOPS;
        }

        public void StreamHasStopped(string sStreamName) { _streamStopTries.Remove(sStreamName); }
    }
}
