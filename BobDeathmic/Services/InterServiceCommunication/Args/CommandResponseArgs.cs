using BobDeathmic.Data.Enums;

namespace BobDeathmic.Eventbus
{
    public class CommandResponseArgs
    {
        public ChatType Chat { get; set; }
        public string Message { get; set; }
        public MessageType MessageType { get; set; }
        public EventType EventType { get; set; }

        public string Sender { get; set; }
        public CommandResponseArgs(ChatType chat,string message,MessageType messageType,EventType eventType,string sender)
        {
            Chat = chat;
            Message = message;
            MessageType = messageType;
            EventType = eventType;
            Sender = sender;
        }
    }
    public enum MessageType
    {
        PrivateMessage = 0,
        ChannelMessage = 1
    }
}