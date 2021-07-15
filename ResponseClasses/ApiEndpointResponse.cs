using System;
using System.Collections.Generic;

namespace PastebinMachine.AutoUpdate.CryptoTool.ResponseClasses
{
	public class ApiEndpointResponse
	{
		public List<ApiEndpointResponse.EndpointData> endpoints;
		public class EndpointData
		{
			public string path;
			public string type;
			public string description;
			public object request;
			public object response;
			public Dictionary<string, object> headers;
			public List<string> flags;
		}
	}
}
