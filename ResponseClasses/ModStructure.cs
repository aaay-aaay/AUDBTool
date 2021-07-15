using System;

namespace PastebinMachine.AutoUpdate.CryptoTool.ResponseClasses
{
	public class ModStructure
	{
		public int version;
		public string url;
		public string sig;
		public ModStructure.ModMetadata metadata;
		public class ModMetadata
		{
			public string name;
			public string type;
			public string description;
			public string readme;
			public object other;
			public bool autoupdate;
			public string filename;
		}
	}
}
