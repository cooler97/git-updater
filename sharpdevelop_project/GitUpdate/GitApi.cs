using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using RestSharp.Authenticators;

namespace GitUpdate
{
	public class GitApi
	{
		const string USER_AGENT = "okno.rf app";
		
		const string ApiUrl = "https://api.github.com";
		const string RawUrl = "https://raw.githubusercontent.com";
		const string RepName = "oknorf";
		const string login = "cooler97";

		readonly IRestClient _client;
		readonly IRestClient _clientContent;
		
		private string accessToken;
		private Updater updater;

		public GitApi(string accessToken, Updater updater)
		{
			this.accessToken = accessToken;
			
			IAuthenticator auth = new HttpBasicAuthenticator(login, accessToken);
			
			_client = new RestClient(ApiUrl);
			_client.UserAgent = USER_AGENT;
			_client.Authenticator = auth;
			
			_clientContent = new RestClient(RawUrl);
			_clientContent.UserAgent = USER_AGENT;
			_clientContent.Authenticator = auth;
			
			this.updater = updater;
		}

		private T Execute<T>(RestRequest request) where T : new()
		{
			IRestResponse<T> response = null;
			
			try
			{
				response = _client.Execute<T>(request);
			}
			catch(WebException e)
			{
				updater.error = e;
				Utils.SendErrorEmailToHelpDesk(e);
				return default(T);
			}

			if (response.ErrorException != null)
			{
				const string message = "Error retrieving response. Check inner details for more info.";
				Exception e = new Exception(message, response.ErrorException);
				updater.error = e;
				Utils.SendErrorEmailToHelpDesk(e);
				return default(T);
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
//			request.AddHeader("Authorization", String.Format("token {0}", accessToken));
			return request;
		}

	}
}
