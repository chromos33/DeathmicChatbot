using BobDeathmic.Data.Enums;

namespace BobDeathmic.Eventbus
{
    public class CommandResponseArgs
    {
        public ChatType Chat { get; set; }
        public string Message { get; set; }
        public MessageType MessageType { get; set; }

        public string Sender { get; set; }
        public string Channel { get; set; }
        public CommandResponseArgs(ChatType chat,string message,MessageType messageType,string sender,string channel)
        {
            Chat = chat;
            Message = message;
            MessageType = messageType;
            Sender = sender;
            Channel = channel;
        }
        public CommandResponseArgs()
        {

        }
    }
    public enum MessageType
    {
        PrivateMessage = 0,
        ChannelMessage = 1
    }
}