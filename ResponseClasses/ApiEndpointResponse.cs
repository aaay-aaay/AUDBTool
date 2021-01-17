using System;
using System.Collections.Generic;

namespace PastebinMachine.AutoUpdate.CryptoTool.ResponseClasses
{
	// Token: 0x02000003 RID: 3
	public class ApiEndpointResponse
	{
		// Token: 0x04000005 RID: 5
		public List<ApiEndpointResponse.EndpointData> endpoints;

		// Token: 0x02000004 RID: 4
		public class EndpointData
		{
			// Token: 0x04000006 RID: 6
			public string path;

			// Token: 0x04000007 RID: 7
			public string type;

			// Token: 0x04000008 RID: 8
			public string description;

			// Token: 0x04000009 RID: 9
			public object request;

			// Token: 0x0400000A RID: 10
			public object response;

			// Token: 0x0400000B RID: 11
			public Dictionary<string, object> headers;

			// Token: 0x0400000C RID: 12
			public List<string> flags;
		}
	}
}
