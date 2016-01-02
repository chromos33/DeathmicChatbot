using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using DeathmicChatbot.Interfaces;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using DeathmicChatbot.StreamInfo;
using DeathmicChatbot.StreamInfo.Hitbox;
using DeathmicChatbot.StreamInfo.Twitch;

namespace DeathmicChatbot.Statics
{
    public static class Stream_Static_Functions
    {
        public static bool isStreamOnline(string sStream)
        {
            bool streamonline = false;
            if(HitboxOnlineState(sStream))
            {
                streamonline = true;
            }
            if (TwitchOnlineState(sStream))
            {
                streamonline = true;
            }

            return streamonline;
        }
        private static bool HitboxOnlineState(string sStream)
        {
            var req = new RestRequest("/media/live/" + sStream, Method.GET);
            IRestClientProvider _restClientProvider = new RestClientProvider(new RestClient("http://api.hitbox.tv"));
            var response = _restClientProvider.Execute(req);
            try
            {
                var des = new JsonDeserializer();

                var data = des.Deserialize<HitboxRootObject>(response);
                foreach (var item in data.livestream)
                {
                    if(item.media_user_name.ToLower() == sStream.ToLower())
                    {
                        if(item.media_is_live == "1")
                        {
                            return true;
                        }
                    }
                }
                return false;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private static bool TwitchOnlineState(string sStream)
        {
            RestClient _client = new RestClient("https://api.twitch.tv");
            var req = new RestRequest("/kraken/streams", Method.GET);
            req.AddParameter("channel", sStream);
            var response = _client.Execute(req);
            try
            {
                var des = new JsonDeserializer();
                var data = des.Deserialize<TwitchRootObject>(response);
                if(data.Streams.Count()>0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
