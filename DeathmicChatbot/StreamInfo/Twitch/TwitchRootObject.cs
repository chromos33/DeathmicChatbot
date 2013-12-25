#region Using

using System.Collections.Generic;

#endregion


namespace DeathmicChatbot.StreamInfo.Twitch
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable InconsistentNaming

    public class TwitchRootObject
    {
        public List<Stream> Streams { get; set; }
        public Links3 Links { get; set; }
    }

    // ReSharper restore InconsistentNaming
    // ReSharper restore ClassNeverInstantiated.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore UnusedMember.Global
}