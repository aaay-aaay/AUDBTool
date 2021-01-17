using System;
using System.Security.Cryptography;

namespace PastebinMachine.AutoUpdate.CryptoTool.ResponseClasses
{
	// Token: 0x02000006 RID: 6
	public class UpdateURLResponse
	{
		// Token: 0x06000012 RID: 18 RVA: 0x00003600 File Offset: 0x00002600
		public bool VerifySignature(byte[] data)
		{
			RSAParameters parameters = default(RSAParameters);
			parameters.Exponent = Convert.FromBase64String(AUDBTool.keyE);
			parameters.Modulus = Convert.FromBase64String(AUDBTool.keyN);
			RSACryptoServiceProvider rsacryptoServiceProvider = new RSACryptoServiceProvider();
			rsacryptoServiceProvider.ImportParameters(parameters);
			return rsacryptoServiceProvider.VerifyData(data, "SHA512", Convert.FromBase64String(this.sig));
		}

		// Token: 0x0400000E RID: 14
		public int version;

		// Token: 0x0400000F RID: 15
		public string url;

		// Token: 0x04000010 RID: 16
		public string sig;
	}
}
