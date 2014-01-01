#region Using

using DeathmicChatbot.Interfaces;
using RestSharp;

#endregion


namespace DeathmicChatbot
{
    public class RestClientProvider : IRestClientProvider
    {
        private readonly RestClient _restClient;

        public RestClientProvider(RestClient restClient) { _restClient = restClient; }

        #region IRestClientProvider Members

        public IRestResponse Execute(IRestRequest req) { return _restClient.Execute(req); }

        #endregion
    }
}