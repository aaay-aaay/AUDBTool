using System;
using System.Collections.Generic;

namespace PastebinMachine.AutoUpdate.CryptoTool.ResponseClasses
{
	// Token: 0x02000007 RID: 7
	public class KeyStructure
	{
		// Token: 0x04000011 RID: 17
		public string e;

		// Token: 0x04000012 RID: 18
		public string n;

		// Token: 0x04000013 RID: 19
		public KeyStructure.KeyAUDBStructure audb;

		// Token: 0x04000014 RID: 20
		public KeyStructure.KeyMetadata metadata;

		// Token: 0x02000008 RID: 8
		public class KeyAUDBStructure
		{
			// Token: 0x04000015 RID: 21
			public List<ModStructure> mods;
		}

		// Token: 0x02000009 RID: 9
		public class KeyMetadata
		{
			// Token: 0x04000016 RID: 22
			public string name;
		}
	}
}
