#region Using

using RestSharp;

#endregion


namespace DeathmicChatbot.Interfaces
{
    public interface IRestClientProvider
    {
        IRestResponse Execute(IRestRequest req);
    }
}