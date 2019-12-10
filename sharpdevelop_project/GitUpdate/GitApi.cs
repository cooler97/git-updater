using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;

namespace GitUpdate
{

    public class GitApi
    {
        const string OAUTH_TOKEN = "16e335be74999ef1e08edd14ce603f2da392fa2d";
        const string USER_AGENT = "okno.rf app";
        
        const string ApiUrl = "https://api.github.com";
        const string RawUrl = "https://raw.githubusercontent.com";
        const string RepName = "oknorf";
        const string login = "cooler97";

        readonly IRestClient _client;
        readonly IRestClient _clientContent;

        public GitApi()
        {
            _client = new RestClient(ApiUrl);
            _client.UserAgent = USER_AGENT;
            
            _clientContent = new RestClient(RawUrl);
            _clientContent.UserAgent = USER_AGENT;
            
            ServicePointManager.SecurityProtocol = SecurityProtocolTypeExtensions.Tls12 | SecurityProtocolTypeExtensions.Tls11;
        }

        private T Execute<T>(RestRequest request) where T : new()
        {
            var response = _client.Execute<T>(request);
            
            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                Exception gitException = new Exception(message, response.ErrorException);
                throw gitException;
            }
            return response.Data;
        }
        
        public List<Content> GetUpdate(string dir)
        {
            RestRequest request = GetRequest("repos/{login}/{repName}/contents/{dir}");
            request.AddUrlSegment("login", login);
            request.AddUrlSegment("repName", RepName);
            request.AddUrlSegment("dir", dir);
            return Execute<List<Content>>(request);
        }
        
        public Content GetReadme()
        {
            RestRequest request = GetRequest("repos/{login}/{repName}/readme");
            request.AddUrlSegment("login", login);
            request.AddUrlSegment("repName", RepName);
            return Execute<Content>(request);
        }
        
        public byte[] Download(string url)
        {
            RestRequest request = GetRequest("{login}/{repName}/master/{path}");
            request.AddUrlSegment("login", login);
            request.AddUrlSegment("repName", RepName);
            request.AddUrlSegment("path", url);
            return _clientContent.DownloadData(request);
        }
        
        private RestRequest GetRequest(string resource)
        {
            RestRequest request = new RestRequest(resource);
            request.AddHeader("Authorization", String.Format("token {0}", OAUTH_TOKEN));
            return request;
        }

    }
}
