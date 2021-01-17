using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using PastebinMachine.AutoUpdate.CryptoTool.ResponseClasses;

namespace PastebinMachine.AutoUpdate.CryptoTool
{
	// Token: 0x02000002 RID: 2
	internal static class AUDBTool
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00001050
		private static void Main()
		{
			Console.SetIn(new StreamReader(Console.OpenStandardInput(8192), Console.InputEncoding, false, 8192));
			WebClient webClient = new WebClient();
			AUDBTool.CheckUpdate(webClient);
			bool flag = false;
			RSACryptoServiceProvider rsacryptoServiceProvider = new RSACryptoServiceProvider();
			RSAParameters rsaparameters = default(RSAParameters);
			bool flag2 = false;
			if (File.Exists("AUDB_RSA_KEY.txt"))
			{
				if (AUDBTool.Prompt("Existing key found, use it?", new string[]
				{
					"y",
					"n"
				}) == "y")
				{
					rsaparameters = AUDBTool.ReadKey(rsaparameters, "AUDB_RSA_KEY.txt");
					flag2 = true;
					flag = true;
				}
			}
			if (!flag2 && File.Exists("RSA_KEY.txt"))
			{
				if (AUDBTool.Prompt("Existing key from ToolMain found, use it?", new string[]
				{
					"y",
					"n"
				}) == "y")
				{
					rsaparameters = AUDBTool.ReadKey(rsaparameters, "RSA_KEY.txt");
					flag2 = true;
					flag = true;
				}
			}
			if (!flag2)
			{
				if (AUDBTool.Prompt("No key was found, or you chose not to read them. Generate a key?", new string[]
				{
					"y",
					"n"
				}) == "y")
				{
					using (RSACryptoServiceProvider rsacryptoServiceProvider2 = new RSACryptoServiceProvider(4096))
					{
						rsaparameters = rsacryptoServiceProvider2.ExportParameters(true);
					}
					flag2 = true;
				}
			}
			string[] options;
			if (flag2)
			{
				rsacryptoServiceProvider.ImportParameters(rsaparameters);
				options = new string[]
				{
					"save",
					"close",
					"raw",
					"newmod",
					"uploadmod",
					"uploadthumb",
					"sign",
					"versions",
                    "hash"
				};
			}
			else
			{
				options = new string[]
				{
					"close",
					"raw",
					"versions",
                    "hash"
				};
				flag = true;
			}
			string text = null;
			int num = -1;
			int num2 = -1;
			if (flag2)
			{
				num2 = AUDBTool.GetKeyID(rsaparameters, webClient);
			}
			for (;;)
			{
				try
				{
					bool flag3 = text != null;
					string text2 = text ?? AUDBTool.Prompt("What do you want to do?", options);
					switch (text2)
					{
					case "save":
					{
						string text3 = null;
						if (AUDBTool.Prompt("Do you want to save it in the default place? This will let AUDBTool automatically detect it next time, but will overwrite any existing text file called AUDB_RSA_KEY (.txt)", new string[]
						{
							"y",
							"n"
						}) == "y")
						{
							text3 = "AUDB_RSA_KEY.txt";
						}
						while (text3 == null)
						{
							Console.Write("Where do you want to save it then? ");
							text3 = Console.ReadLine();
							if (AUDBTool.Prompt("Are you sure you want to save it in " + text3 + "?", new string[]
							{
								"y",
								"n"
							}) == "n")
							{
								text3 = null;
							}
						}
						AUDBTool.SaveKey(rsaparameters, text3);
						flag = true;
						break;
					}
					case "close":
					{
						string text4 = "Are you sure you want to close the AUDBTool?";
						if (!flag)
						{
							text4 += " You have an unsaved key!";
						}
						if (AUDBTool.Prompt(text4, new string[]
						{
							"y",
							"n"
						}) == "y")
						{
							return;
						}
						break;
					}
					case "raw":
						if (!(AUDBTool.Prompt("You have chosen to interact directly with the API. Are you sure?", new string[]
						{
							"y",
							"n"
						}) == "n"))
						{
							string stringFromURL = AUDBTool.GetStringFromURL(webClient, "http://beestuff.pythonanywhere.com/api");
							JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
							ApiEndpointResponse apiEndpointResponse = javaScriptSerializer.Deserialize<ApiEndpointResponse>(stringFromURL);
							foreach (ApiEndpointResponse.EndpointData endpointData in apiEndpointResponse.endpoints)
							{
								Console.WriteLine("Endpoint at " + endpointData.type + " " + endpointData.path);
							}
							bool flag4 = false;
							while (!flag4)
							{
								Console.Write("Enter an endpoint, or anything else to go back to the main AUDBTool. ");
								string text5 = Console.ReadLine();
								ApiEndpointResponse.EndpointData endpointData2 = null;
								foreach (ApiEndpointResponse.EndpointData endpointData in apiEndpointResponse.endpoints)
								{
									if (endpointData.type + " " + endpointData.path == text5)
									{
										endpointData2 = endpointData;
									}
								}
								if (endpointData2 == null)
								{
									if (AUDBTool.Prompt("Do you want to quit the raw interaction?", new string[]
									{
										"y",
										"n"
									}) == "y")
									{
										flag4 = true;
										break;
									}
								}
								else if (endpointData2.flags.Contains("key") && !flag2)
								{
									Console.WriteLine("This endpoint requires a key, but you do not have one.");
								}
								else
								{
									string a = text5.Split(new char[]
									{
										' '
									})[0];
									string str = AUDBTool.DoFormat(text5.Split(new char[]
									{
										' '
									})[1]);
									if (a != "GET")
									{
										Console.WriteLine("Currently only GET requests are supported");
									}
									else
									{
										if (endpointData2.flags.Contains("auth"))
										{
											AUDBTool.DoAuthstring(webClient, rsacryptoServiceProvider, rsaparameters, javaScriptSerializer);
										}
										if (endpointData2.flags.Contains("key"))
										{
											webClient.Headers.Set("Authorization-E", Convert.ToBase64String(rsaparameters.Exponent));
											webClient.Headers.Set("Authorization-N", Convert.ToBase64String(rsaparameters.Modulus));
										}
										if (a == "GET")
										{
											string stringFromURL2 = AUDBTool.GetStringFromURL(webClient, "http://beestuff.pythonanywhere.com" + str);
											Console.WriteLine("The response: " + stringFromURL2);
										}
									}
								}
							}
						}
						break;
					case "newmod":
					{
						string stringFromURL = AUDBTool.GetStringFromURL(webClient, "http://beestuff.pythonanywhere.com/api");
						JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
						ApiEndpointResponse apiEndpointResponse = javaScriptSerializer.Deserialize<ApiEndpointResponse>(stringFromURL);
						ApiEndpointResponse.EndpointData endpointData3 = null;
						foreach (ApiEndpointResponse.EndpointData endpointData in apiEndpointResponse.endpoints)
						{
							if (endpointData.path == "/audb/api/mods/new")
							{
								endpointData3 = endpointData;
							}
						}
						if (endpointData3 == null)
						{
							Console.WriteLine("Mod creation endpoint couldn't be found, sorry!");
							Console.WriteLine("This is probably an internal problem with the API, or you have an outdated version of AUDBTool.");
							if (AUDBTool.Prompt("Do you want to quit the tool now?", new string[]
							{
								"y",
								"n"
							}) == "y")
							{
								return;
							}
						}
						else
						{
							AUDBTool.DoAuthstring(webClient, rsacryptoServiceProvider, rsaparameters, javaScriptSerializer);
							Dictionary<string, object> dictionary = new Dictionary<string, object>();
							foreach (string text6 in ((Dictionary<string, object>)endpointData3.request).Keys)
							{
								if (((Dictionary<string, object>)endpointData3.request)[text6] is string && (string)((Dictionary<string, object>)endpointData3.request)[text6] == "string")
								{
									Console.Write("Please enter " + text6 + ": ");
									dictionary[text6] = Console.ReadLine();
								}
								else if (((Dictionary<string, object>)endpointData3.request)[text6] is string && (string)((Dictionary<string, object>)endpointData3.request)[text6] == "bool")
								{
									dictionary[text6] = (AUDBTool.Prompt(text6 + "?", new string[]
									{
										"y",
										"n"
									}) == "y");
								}
								else
								{
									dictionary[text6] = null;
								}
							}
							string text7 = AUDBTool.PostStringToURL(webClient, "http://beestuff.pythonanywhere.com/audb/api/mods/new", javaScriptSerializer.Serialize(dictionary));
							Console.WriteLine("// Code for AutoUpdate support");
							Console.WriteLine("// Should be put in the main PartialityMod class.");
							Console.WriteLine("// Comments are optional.");
							Console.WriteLine();
							Console.WriteLine("// Update URL - don't touch!");
							Console.WriteLine("// You can go to this in a browser (it's safe), but you might not understand the result.");
							Console.WriteLine("// This URL is specific to this mod, and identifies it on AUDB.");
							Console.WriteLine("public string updateURL = \"{0}\";", text7);
							Console.WriteLine("// Version - increase this by 1 when you upload a new version of the mod.");
							Console.WriteLine("// The first upload should be with version 0, the next version 1, the next version 2, etc.");
							Console.WriteLine("// If you ever lose track of the version you're meant to be using, ask Pastebin.");
							Console.WriteLine("public int version = 0;");
							Console.WriteLine("// Public key in base64 - don't touch!");
							Console.WriteLine("public string keyE = \"{0}\";", Convert.ToBase64String(rsaparameters.Exponent));
							Console.WriteLine("public string keyN = \"{0}\";", Convert.ToBase64String(rsaparameters.Modulus));
							Console.WriteLine("// ------------------------------------------------");
							Console.WriteLine("If you include the code above and upload the mod, it will immediately be able to support AutoUpdate.");
							if (AUDBTool.Prompt("Would you like to upload the mod?", new string[]
							{
								"y",
								"n"
							}) == "y")
							{
								text = "uploadmod";
								string[] array = text7.Split(new char[]
								{
									'/'
								});
								num = int.Parse(array[array.Length - 1]);
							}
						}
						break;
					}
					case "uploadmod":
					{
						JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
						string stringFromURL3 = AUDBTool.GetStringFromURL(webClient, "http://beestuff.pythonanywhere.com/keydb/api/keys/" + num2.ToString());
						KeyStructure keyStructure = javaScriptSerializer.Deserialize<KeyStructure>(stringFromURL3);
						for (int i = 0; i < keyStructure.audb.mods.Count; i++)
						{
							Console.WriteLine("Mod ID {0}: {1}", i, keyStructure.audb.mods[i].metadata.name);
						}
						if (num == -1)
						{
							num = AUDBTool.PromptInt("Please enter the mod ID you want to upload.");
						}
						Console.Write("Please enter the filename of the mod to upload: ");
						string text8 = Console.ReadLine();
						byte[] buffer = File.ReadAllBytes(text8);
						byte[] inArray = rsacryptoServiceProvider.SignData(buffer, "SHA512");
						string value = Convert.ToBase64String(inArray);
						AUDBTool.DoAuthstring(webClient, rsacryptoServiceProvider, rsaparameters, javaScriptSerializer);
						webClient.Headers.Set("modsig", value);
						AUDBTool.PostFileToURL(webClient, "http://beestuff.pythonanywhere.com/audb/api/mods/" + num.ToString() + "/upload", text8);
						num = -1;
						break;
					}
					case "uploadthumb":
					{
						JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
						string stringFromURL4 = AUDBTool.GetStringFromURL(webClient, "http://beestuff.pythonanywhere.com/keydb/api/keys/" + num2.ToString());
						KeyStructure keyStructure2 = javaScriptSerializer.Deserialize<KeyStructure>(stringFromURL4);
						for (int i = 0; i < keyStructure2.audb.mods.Count; i++)
						{
							Console.WriteLine("Mod ID {0}: {1}", i, keyStructure2.audb.mods[i].metadata.name);
						}
						if (num == -1)
						{
							num = AUDBTool.PromptInt("Please enter the mod ID you want to upload the thumbnail (image) of.");
						}
						Console.Write("Please enter the filename of the image you want to upload: ");
						string text9 = Console.ReadLine();
						byte[] array2 = File.ReadAllBytes(text9);
						AUDBTool.DoAuthstring(webClient, rsacryptoServiceProvider, rsaparameters, javaScriptSerializer);
						AUDBTool.PostFileToURL(webClient, "http://beestuff.pythonanywhere.com/audb/api/mods/" + num.ToString() + "/thumb", text9);
						num = -1;
						break;
					}
					case "sign":
					{
						Console.WriteLine("WARNING: Signing data is the only method of authorization for AUDB. ONLY do this if Pastebin has manually asked you to, or you know EXACTLY what you are doing! NO EXCEPTIONS!");
						Console.Write("anyway now that we've done that warning, what do you actually want to sign? (there's no confirmation so input it right the first time please) ");
						string s = Console.ReadLine();
						byte[] inArray2 = rsacryptoServiceProvider.SignData(Encoding.ASCII.GetBytes(s), "SHA512");
						string str2 = Convert.ToBase64String(inArray2);
						Console.WriteLine("signature: " + str2);
						break;
					}
					case "versions":
					{
						JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
						string stringFromURL5 = AUDBTool.GetStringFromURL(webClient, "http://beestuff.pythonanywhere.com/keydb/api/keys");
						List<KeyStructure> list = javaScriptSerializer.Deserialize<List<KeyStructure>>(stringFromURL5);
						for (int i = 0; i < list.Count; i++)
						{
							KeyStructure keyStructure3 = list[i];
							Console.WriteLine("KEY {0} ({1})", i, keyStructure3.metadata.name);
							for (int j = 0; j < keyStructure3.audb.mods.Count; j++)
							{
								ModStructure modStructure = keyStructure3.audb.mods[j];
								Console.WriteLine("{0} ({1}) - version {2}", j, modStructure.metadata.name, modStructure.version);
							}
						}
						break;
					}
                    case "hash":
                    {
                        Console.WriteLine("Warning: This is intended for Bee's use only, so it's quite unfriendly");
                        string filename = Console.ReadLine();
                        byte[] data = File.ReadAllBytes(filename);
                        byte[] hash;
                        using (SHA512 shaM = new SHA512Managed())
                        {
                            hash = shaM.ComputeHash(data);
                        }
                        Console.WriteLine(Convert.ToBase64String(hash));
                        break;
                    }
					}
					if (flag3)
					{
						text = null;
					}
				}
				catch (WebException ex)
				{
					Console.WriteLine("ERROR RESPONSE:");
					Console.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
				}
			}
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002F44 File Offset: 0x00001F44
		private static void DoAuthstring(WebClient client, RSACryptoServiceProvider rsa, RSAParameters rsaParams, JavaScriptSerializer jss)
		{
			client.Headers.Set("Authorization-E", Convert.ToBase64String(rsaParams.Exponent));
			client.Headers.Set("Authorization-N", Convert.ToBase64String(rsaParams.Modulus));
			string stringFromURL = AUDBTool.GetStringFromURL(client, "http://beestuff.pythonanywhere.com/keydb/api/auth/get");
			string authstring = jss.Deserialize<AuthgetResponse>(stringFromURL).authstring;
			string text = "authsign:" + authstring;
			Console.WriteLine("signing " + text);
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			byte[] inArray = rsa.SignData(bytes, "SHA512");
			string value = Convert.ToBase64String(inArray);
			client.Headers.Set("authstring", authstring);
			client.Headers.Set("sig", value);
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000300C File Offset: 0x0000200C
		private static string Prompt(string prompt, string[] options)
		{
			string result;
			if (options.Length == 0)
			{
				result = "";
			}
			else
			{
				Console.Write("{0} [", prompt);
				Console.Write(options[0]);
				for (int i = 1; i < options.Length; i++)
				{
					Console.Write("/");
					Console.Write(options[i]);
				}
				Console.Write("] ");
				string text = Console.ReadLine();
				if (!options.Contains(text))
				{
					result = AUDBTool.Prompt("That was not a valid choice. " + prompt, options);
				}
				else
				{
					result = text;
				}
			}
			return result;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000030A4 File Offset: 0x000020A4
		private static int PromptInt(string prompt)
		{
			Console.Write("{0} ", prompt);
			string s = Console.ReadLine();
			int result;
			try
			{
				result = int.Parse(s);
			}
			catch
			{
				result = AUDBTool.PromptInt("That was not a valid number. " + prompt);
			}
			return result;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000030F8 File Offset: 0x000020F8
		private static RSAParameters ReadKey(RSAParameters rsaParams, string filename)
		{
			string[] array = File.ReadAllText(filename).Split(new char[]
			{
				' '
			});
			rsaParams.Exponent = Convert.FromBase64String(array[0]);
			rsaParams.Modulus = Convert.FromBase64String(array[1]);
			rsaParams.D = Convert.FromBase64String(array[2]);
			rsaParams.DP = Convert.FromBase64String(array[3]);
			rsaParams.DQ = Convert.FromBase64String(array[4]);
			rsaParams.InverseQ = Convert.FromBase64String(array[5]);
			rsaParams.P = Convert.FromBase64String(array[6]);
			rsaParams.Q = Convert.FromBase64String(array[7]);
			Console.WriteLine("READ KEY");
			Console.WriteLine("e = " + array[0]);
			Console.WriteLine("n = " + array[1]);
			return rsaParams;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000031D0 File Offset: 0x000021D0
		private static string KeySaveString(RSAParameters rsaParams)
		{
			return string.Concat(new string[]
			{
				Convert.ToBase64String(rsaParams.Exponent),
				" ",
				Convert.ToBase64String(rsaParams.Modulus),
				" ",
				Convert.ToBase64String(rsaParams.D),
				" ",
				Convert.ToBase64String(rsaParams.DP),
				" ",
				Convert.ToBase64String(rsaParams.DQ),
				" ",
				Convert.ToBase64String(rsaParams.InverseQ),
				" ",
				Convert.ToBase64String(rsaParams.P),
				" ",
				Convert.ToBase64String(rsaParams.Q)
			});
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000032A8 File Offset: 0x000022A8
		private static void SaveKey(RSAParameters rsaParams, string filename)
		{
			string contents = AUDBTool.KeySaveString(rsaParams);
			File.WriteAllText(filename, contents);
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000032C8 File Offset: 0x000022C8
		private static string GetStringFromURL(WebClient client, string url)
		{
			Stream stream = client.OpenRead(url);
			StreamReader streamReader = new StreamReader(stream);
			string result = streamReader.ReadToEnd();
			stream.Close();
			streamReader.Close();
			return result;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00003300 File Offset: 0x00002300
		private static string PostStringToURL(WebClient client, string url, string data)
		{
			client.Headers[HttpRequestHeader.ContentType] = "application/json";
			return client.UploadString(url, data);
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00003330 File Offset: 0x00002330
		private static byte[] PostFileToURL(WebClient client, string url, string file)
		{
			return client.UploadFile(url, file);
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000334C File Offset: 0x0000234C
		private static string DoFormat(string url)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num;
			while ((num = url.IndexOf('{')) >= 0)
			{
				stringBuilder.Append(url.Substring(0, num));
				url = url.Substring(num + 1);
				num = url.IndexOf(':');
				string text = url.Substring(0, num);
				url = url.Substring(num + 1);
				num = url.IndexOf('}');
				string text2 = url.Substring(0, num);
				url = url.Substring(num + 1);
				Console.Write(string.Concat(new string[]
				{
					"Please enter ",
					text,
					" (a ",
					text2,
					") "
				}));
				string value = Console.ReadLine();
				stringBuilder.Append(value);
			}
			stringBuilder.Append(url);
			return stringBuilder.ToString();
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00003434 File Offset: 0x00002434
		private static void CheckUpdate(WebClient client)
		{
			JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
			string stringFromURL = AUDBTool.GetStringFromURL(client, AUDBTool.updateURL);
			UpdateURLResponse updateURLResponse = javaScriptSerializer.Deserialize<UpdateURLResponse>(stringFromURL);
			if (updateURLResponse.version < AUDBTool.version)
			{
				Console.WriteLine("Note: This version of AUDBTool is greater than the current latest on AUDB.");
			}
			if (updateURLResponse.version > AUDBTool.version)
			{
				Console.WriteLine("Downloading update...");
				client.DownloadFile(updateURLResponse.url, "AUDBTool.UNCONFIRMED.exe");
				byte[] data = File.ReadAllBytes("AUDBTool.UNCONFIRMED.exe");
				if (!updateURLResponse.VerifySignature(data))
				{
					Console.WriteLine("INCORRECT SIGNATURE!!!");
					File.Delete("AUDBTool.UNCONFIRMED.exe");
				}
				else
				{
					Console.WriteLine("Update downloaded! Restarting...");
					Process.Start(new ProcessStartInfo("cmd.exe")
					{
						Arguments = "/c move /Y AUDBTool.UNCONFIRMED.exe AUDBTool.exe"
					});
					Environment.Exit(0);
				}
			}
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00003518 File Offset: 0x00002518
		private static int GetKeyID(RSAParameters rsaParams, WebClient client)
		{
			JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
			string stringFromURL = AUDBTool.GetStringFromURL(client, "http://beestuff.pythonanywhere.com/keydb/api/keys");
			List<KeyStructure> list = javaScriptSerializer.Deserialize<List<KeyStructure>>(stringFromURL);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].e == Convert.ToBase64String(rsaParams.Exponent) && list[i].n == Convert.ToBase64String(rsaParams.Modulus))
				{
					Console.WriteLine("Using key {0}", i);
					return i;
				}
			}
			return -1;
		}

		// Token: 0x04000001 RID: 1
		public static string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/0/7";

		// Token: 0x04000002 RID: 2
		public static int version = 14;

		// Token: 0x04000003 RID: 3
		public static string keyE = "AQAB";

		// Token: 0x04000004 RID: 4
		public static string keyN = "yu7XMmICrzuavyZRGWoknFIbJX4N4zh3mFPOyfzmQkil2axVIyWx5ogCdQ3OTdSZ0xpQ3yiZ7zqbguLu+UWZMfLOBKQZOs52A9OyzeYm7iMALmcLWo6OdndcMc1Uc4ZdVtK1CRoPeUVUhdBfk2xwjx+CvZUlQZ26N1MZVV0nq54IOEJzC9qQnVNgeeHxO1lRUTdg5ZyYb7I2BhHfpDWyTvUp6d5m6+HPKoalC4OZSfmIjRAi5UVDXNRWn05zeT+3BJ2GbKttwvoEa6zrkVuFfOOe9eOAWO3thXmq9vJLeF36xCYbUJMkGR2M5kDySfvoC7pzbzyZ204rXYpxxXyWPP5CaaZFP93iprZXlSO3XfIWwws+R1QHB6bv5chKxTZmy/Imo4M3kNLo5B2NR/ZPWbJqjew3ytj0A+2j/RVwV9CIwPlN4P50uwFm+Mr0OF2GZ6vU0s/WM7rE78+8Wwbgcw6rTReKhVezkCCtOdPkBIOYv3qmLK2S71NPN2ulhMHD9oj4t0uidgz8pNGtmygHAm45m2zeJOhs5Q/YDsTv5P7xD19yfVcn5uHpSzRIJwH5/DU1+aiSAIRMpwhF4XTUw73+pBujdghZdbdqe2CL1juw7XCa+XfJNtsUYrg+jPaCEUsbMuNxdFbvS0Jleiu3C8KPNKDQaZ7QQMnEJXeusdU=";
	}
}
