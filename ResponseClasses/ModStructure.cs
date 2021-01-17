using System;

namespace PastebinMachine.AutoUpdate.CryptoTool.ResponseClasses
{
	// Token: 0x0200000A RID: 10
	public class ModStructure
	{
		// Token: 0x04000017 RID: 23
		public int version;

		// Token: 0x04000018 RID: 24
		public string url;

		// Token: 0x04000019 RID: 25
		public string sig;

		// Token: 0x0400001A RID: 26
		public ModStructure.ModMetadata metadata;

		// Token: 0x0200000B RID: 11
		public class ModMetadata
		{
			// Token: 0x0400001B RID: 27
			public string name;

			// Token: 0x0400001C RID: 28
			public string type;

			// Token: 0x0400001D RID: 29
			public string description;

			// Token: 0x0400001E RID: 30
			public string readme;

			// Token: 0x0400001F RID: 31
			public object other;

			// Token: 0x04000020 RID: 32
			public bool autoupdate;

			// Token: 0x04000021 RID: 33
			public string filename;
		}
	}
}
