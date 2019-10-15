using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BobDeathmic.JSONObjects
{
    public class MixerChannelInfo
    {
        public bool featured { get; set; }
        public int? id { get; set; }
        public int? userId { get; set; }
        public string token { get; set; }
        public bool online { get; set; }
        public int? featureLevel { get; set; }
        public bool partnered { get; set; }
        public int? transcodingProfileId { get; set; }
        public bool suspended { get; set; }
        public string name { get; set; }
        public string audience { get; set; }
        public int? viewersTotal { get; set; }
        public int? viewersCurrent { get; set; }
        public int? numFollowers { get; set; }
        public string description { get; set; }
        public int? typeId { get; set; }
        public bool interactive { get; set; }
        public int? interactiveGameId { get; set; }
        public int? ftl { get; set; }
        public bool hasVod { get; set; }
        public int? langaugeId { get; set; }
        public int? coverId { get; set; }
        public int? thumbnailId { get; set; }
        public int? badgeId { get; set; }
        public string bannerUrl { get; set; }
        public int? hosteeId { get; set; }
        public bool hasTranscodes { get; set; }
        public bool vodsEnabled { get; set; }
        public int? costreamId { get; set; }
        public DateTime? createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
        public DateTime? deletedAt { get; set; }
        public string thumbnail { get; set; }
        public MixerType type { get; set; }
    }
    public class MixerType
    {
        public string name { get; set; }
    }
}
