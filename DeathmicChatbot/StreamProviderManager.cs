#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DeathmicChatbot.Interfaces;
using DeathmicChatbot.Properties;

#endregion


namespace DeathmicChatbot
{
    public class StreamProviderManager
    {
        private readonly Thread _streamCheckThread;
        private readonly List<IStreamProvider> _streamProviders =
            new List<IStreamProvider>();

        public StreamProviderManager()
        {
            _streamCheckThread = new Thread(CheckAllStreamsThreaded);
            _streamCheckThread.Start();
        }

        private void CheckAllStreamsThreaded()
        {
            while (true)
            {
                foreach (var streamProvider in _streamProviders)
                    streamProvider.CheckStreams();

                Thread.Sleep(Settings.Default.StreamcheckIntervalSeconds * 1000);
            }
        }

        public void AddStreamProvider(IStreamProvider streamProvider)
        {
            _streamProviders.Add(streamProvider);

            streamProvider.StreamStarted += (sender, args) =>
                                            {
                                                if (StreamStarted != null)
                                                    StreamStarted(sender, args);
                                            };

            streamProvider.StreamStopped += (sender, args) =>
                                            {
                                                if (StreamStopped != null)
                                                    StreamStopped(sender, args);
                                            };
        }

        public bool AddStream(string sStreamName)
        {
            var results = new Dictionary<IStreamProvider, bool>();

            foreach (var streamProvider in _streamProviders)
            {
                var bResult = streamProvider.AddStream(sStreamName);
                results.Add(streamProvider, bResult);
            }

            return results.All(result => !result.Value);
        }

        public void RemoveStream(string sStreamName)
        {
            foreach (var streamProvider in _streamProviders)
                streamProvider.RemoveStream(sStreamName);
        }

        public IEnumerable<string> GetStreamInfoArray()
        {
            var streamInfoArrays = new List<string>();

            foreach (var streamInfoArray in
                _streamProviders.Select(
                    streamProvider => streamProvider.GetStreamInfoArray()))
                streamInfoArrays.AddRange(streamInfoArray);

            return streamInfoArrays;
        }

        public event EventHandler<StreamEventArgs> StreamStarted;
        public event EventHandler<StreamEventArgs> StreamStopped;
    }
}