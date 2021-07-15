using System;
using System.Collections.Generic;

namespace PastebinMachine.AutoUpdate.CryptoTool.ResponseClasses
{
	public class KeyStructure
	{
		public string e;
		public string n;
		public KeyStructure.KeyAUDBStructure audb;
		public KeyStructure.KeyMetadata metadata;
		public class KeyAUDBStructure
		{
			public List<ModStructure> mods;
		}
		public class KeyMetadata
		{
			public string name;
		}
	}
}
