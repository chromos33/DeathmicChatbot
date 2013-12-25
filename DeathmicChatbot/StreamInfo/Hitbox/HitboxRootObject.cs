#region Using

using System.Collections.Generic;

#endregion


namespace DeathmicChatbot.StreamInfo.Hitbox
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable ClassNeverInstantiated.Global
    // ReSharper disable InconsistentNaming

    internal class HitboxRootObject
    {
        public Request request { get; set; }
        public string media_type { get; set; }
        public List<Livestream> livestream { get; set; }
    }

    // ReSharper restore InconsistentNaming
    // ReSharper restore ClassNeverInstantiated.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore UnusedMember.Global
}