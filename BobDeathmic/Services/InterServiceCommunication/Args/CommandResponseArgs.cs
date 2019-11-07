using BobDeathmic.Data.Enums;

namespace BobDeathmic.Eventbus
{
    public class CommandResponseArgs
    {
        public ChatType Chat { get; set; }
        public string Message { get; set; }
        public MessageType MessageType { get; set; }
        public EventType EventType { get; set; }
        public CommandResponseArgs(ChatType chat,string message,MessageType messageType,EventType eventType)
        {
            Chat = chat;
            Message = message;
            MessageType = messageType;
            EventType = eventType;
        }
    }
    public enum MessageType
    {
        PrivateMessage = 0,
        ChannelMessage = 1
    }
}