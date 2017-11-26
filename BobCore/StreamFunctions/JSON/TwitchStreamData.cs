using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BobCore.StreamFunctions.JSON
{

    #region api 6
    //Just Data Class for Twitch api values are never asigned outside of json decoding
    class TwitchStreamData
    {
#pragma warning disable 0649
        public TwitchStream[] data;
#pragma warning restore 0649
    }
    class TwitchStream
    {
#pragma warning disable 0649
        public string id;
        public string user_id;
        public string game_id;
        public string[] community_ids;
        public string type;
        public string title;
        public string viewer_count;
        public string started_at;
        public string language;
        public string thumbnail_url;
#pragma warning restore 0649
    }
    class pagination
    {
#pragma warning disable 0649
        public string cursor;
#pragma warning restore 0649
    }
    class TwitchID
    {
#pragma warning disable 0649
        public TwitchIDData[] data;
#pragma warning restore 0649
    }
    class TwitchIDData
    {
#pragma warning disable 0649
        public string id;
        public string login;
        public string display_name;
        public string type;
        public string broadcaster_type;
        public string description;
        public string profile_image_url;
        public string offline_image_url;
        public string view_count;
        public string email;
#pragma warning restore 0649
    }
    #endregion
    #region api 5
    /* 
    class TwitchStreamData
    {
        public int _total;
        public TwitchStream[] streams;
        public TwitchLinks _links;
    }
    class TwitchStream
    {
        public Int64 _id;
        public double average_fps;
        public TwitchChannel channel;
        public string created_at;
        public int delay;
        public string game;
        public bool is_playlist;
        public TwitchPreview preview;
        public int video_height;
        public int viewers;

    }
    class TwitchChannel
    {
        public Int64 _id;
        public string broadcaster_language;
        public string created_at;
        public string display_name;
        public int folowers;
        public string game;
        public string language;
        public string logo;
        public bool mature;
        public string name;
        public bool partner;
        public string profile_banner;
        public string profile_banner_background_color;
        public string status;
        public string updated_at;
        public string url;
        public string video_banner;
        public int views;
    }
    class TwitchPreview
    {
        public string large;
        public string medium;
        public string small;
        public string template;
    }
    class TwitchLinks
    {
        public string self;
        public string next;
        public string featured;
        public string summary;
        public string followed;
    }
    */
    #endregion

}
