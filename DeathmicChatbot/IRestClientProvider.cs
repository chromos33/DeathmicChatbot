#region Using

using RestSharp;

#endregion


namespace DeathmicChatbot
{
    public interface IRestClientProvider
    {
        IRestResponse Execute(IRestRequest req);
    }
}