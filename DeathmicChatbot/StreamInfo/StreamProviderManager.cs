using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DeathmicChatbot.Interfaces;
using DeathmicChatbot.Properties;

namespace DeathmicChatbot.StreamInfo
{
    public class StreamProviderManager : IDisposable
    {
        bool disposed = false;
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
            Console.WriteLine("CheckAllStreamsThreaded");
            while (true)
            {
                try
                {
                    foreach (var streamProvider in _streamProviders)
                        streamProvider.CheckStreams();
                } catch (InvalidOperationException)
                {
                    // Do nothing only happens when debug pause..
                }
                

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
            streamProvider.StreamGlobalNotification += (sender, args) =>
            {
                if (StreamGlobalNotification != null)
                {
                    StreamGlobalNotification(sender, args);
                }
            };
            
            if (streamProvider.ToString() == "DeathmicChatbot.StreamInfo.Hitbox.HitboxProvider")
            {
                streamProvider.StartTimer();
            }       


        }
        public bool AddStream(string sStreamName)
        {
            var results = new Dictionary<IStreamProvider, bool>();
            foreach (var streamProvider in _streamProviders)
            {
                results.Add(streamProvider, streamProvider.AddStream(sStreamName));
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
        public event EventHandler<StreamEventArgs> StreamGlobalNotification;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                foreach(var streamprovider in _streamProviders)
                {
                    streamprovider.Dispose();
                }
            }

            // Free any unmanaged objects here. 
            //
            disposed = true;
        }
    }
}
