using System;
using System.Security.Cryptography;

namespace PastebinMachine.AutoUpdate.CryptoTool.ResponseClasses
{
	public class UpdateURLResponse
	{
		public bool VerifySignature(byte[] data)
		{
			RSAParameters parameters = default(RSAParameters);
			parameters.Exponent = Convert.FromBase64String(AUDBTool.keyE);
			parameters.Modulus = Convert.FromBase64String(AUDBTool.keyN);
			RSACryptoServiceProvider rsacryptoServiceProvider = new RSACryptoServiceProvider();
			rsacryptoServiceProvider.ImportParameters(parameters);
			return rsacryptoServiceProvider.VerifyData(data, "SHA512", Convert.FromBase64String(this.sig));
		}
		public int version;
		public string url;
		public string sig;
	}
}
