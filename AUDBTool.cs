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
using System.Text.RegularExpressions;

using static PastebinMachine.AutoUpdate.CryptoTool.ConsoleUtils;
using static PastebinMachine.AutoUpdate.CryptoTool.CryptoCommUtils;

namespace PastebinMachine.AutoUpdate.CryptoTool
{
	internal static class AUDBTool
    {
		private static void Main()
		{
			Console.SetIn(new StreamReader(Console.OpenStandardInput(8192), Console.InputEncoding, false, 8192));
			WebClient wc = new WebClient();
			CheckUpdate(wc);
			bool properKeyFound = false;
			bool TMKeyFound = false;
			RSACryptoServiceProvider rsa_csp = new RSACryptoServiceProvider();
			RSAParameters currentKey = default(RSAParameters);
			if (File.Exists("AUDB_RSA_KEY.txt"))
			{
				if (PromptBinary("Existing key found, use it?"))
				{
					currentKey = ReadKey(currentKey, "AUDB_RSA_KEY.txt");
					properKeyFound = true;
					TMKeyFound = true;
				}
			}
			if (!properKeyFound && File.Exists("RSA_KEY.txt"))
			{
				if (PromptBinary("Found existing key from ToolMain, use it?"))
				{
					currentKey = ReadKey(currentKey, "RSA_KEY.txt");
					properKeyFound = true;
					TMKeyFound = true;
				}
			}
			if (!properKeyFound)
			{
				if (PromptBinary("No key found. Generate a new one?"))
				{
					using (RSACryptoServiceProvider new_csp = new RSACryptoServiceProvider(4096))
					{
						currentKey = new_csp.ExportParameters(true);
					}
					properKeyFound = true;
				}
			}
			string[] options;
			if (properKeyFound)
			{
				rsa_csp.ImportParameters(currentKey);
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
				TMKeyFound = true;
			}
			string QueuedAction = null;
			int currModID = -1;
			int currUID = -1;
			if (properKeyFound)
			{
				currUID = GetKeyID(currentKey, wc);
			}
			for (;;)
			{
				try
				{
					bool flag3 = QueuedAction != null;
					string userResponse = QueuedAction ?? Prompt("What do you want to do?", options);
					switch (userResponse)
					{
						case "save":
							{
								string SelectedKeySaveLoc = null;
								if (PromptBinary("Do you want to save it in the default place? This will let AUDBTool automatically detect it next time, but will overwrite any existing text file called AUDB_RSA_KEY (.txt)"))
								{
									SelectedKeySaveLoc = "AUDB_RSA_KEY.txt";
								}
								while (SelectedKeySaveLoc == null)
								{
									Console.Write("Where do you want to save it then?");
									SelectedKeySaveLoc = Console.ReadLine();

									if (!PromptBinary($"Confirm save location: {SelectedKeySaveLoc}"))
									{
										SelectedKeySaveLoc = null;
									}
								}
								SaveKey(currentKey, SelectedKeySaveLoc);
								TMKeyFound = true;
								break;
							}
						case "close":
							{
								string exitConfMessage = "Are you sure you want to close the AUDBTool?";
								if (!TMKeyFound) exitConfMessage += " You have an unsaved key!";
								if (PromptBinary(exitConfMessage)) return;
								break;
							}
						case "raw":
							if (!PromptBinary("You have chosen to interact directly with the API. Are you sure?"))
							{
								string json;
								ApiEndpointResponse apiEndpointResponse;
								JavaScriptSerializer jss = new JavaScriptSerializer();
								try
								{
									json = GetStringFromURL(wc, "http://beestuff.pythonanywhere.com/api");
									apiEndpointResponse = jss.Deserialize<ApiEndpointResponse>(json);
								}
								catch (Exception e)
								{
									Console.WriteLine("Error fetching AUDB endpoints:");
									Console.WriteLine(e);
									break;
								}

								foreach (ApiEndpointResponse.EndpointData endpointData in apiEndpointResponse.endpoints)
								{
									Console.WriteLine("Endpoint at " + endpointData.type + " " + endpointData.path);
								}
								bool exitFlag = false;
								while (!exitFlag)
								{
									Console.Write("Enter an endpoint, or anything else to go back to the main AUDBTool. ");
									string userInput = Console.ReadLine();
									ApiEndpointResponse.EndpointData SelectedEndpoint = null;
									foreach (ApiEndpointResponse.EndpointData endpointData in apiEndpointResponse.endpoints)
									{
										if (endpointData.type + " " + endpointData.path == userInput)
										{
											SelectedEndpoint = endpointData;
										}
									}
									if (SelectedEndpoint == null)
									{
										if (PromptBinary("Do you want to quit the raw interaction?"))
										{
											exitFlag = true;
											break;
										}
									}
									else if (SelectedEndpoint.flags.Contains("key") && !properKeyFound)
									{
										Console.WriteLine("This endpoint requires a key, but you do not have one.");
									}
									else
									{
										try
										{
											string a = userInput.Split(new char[] { ' ' })[0];
											string str = DoFormat(userInput.Split(new char[] { ' ' })[1]);
											if (a != "GET")
											{
												Console.WriteLine("Currently only GET requests are supported");
											}
											else
											{
												if (SelectedEndpoint.flags.Contains("auth"))
												{
													DoAuthstring(wc, rsa_csp, currentKey, jss);
												}
												if (SelectedEndpoint.flags.Contains("key"))
												{
													wc.Headers.Set("Authorization-E", Convert.ToBase64String(currentKey.Exponent));
													wc.Headers.Set("Authorization-N", Convert.ToBase64String(currentKey.Modulus));
												}
												if (a == "GET")
												{
													string stringFromURL2 = GetStringFromURL(wc, "http://beestuff.pythonanywhere.com" + str);
													Console.WriteLine("The response: " + stringFromURL2);
												}
											}
										}
										catch (Exception e)
										{
											Console.WriteLine("Error during raw interaction:");
											Console.WriteLine(e);

										}

									}
								}
							}
							break;
						case "newmod":
							{
								string json;
								ApiEndpointResponse audbResponse;
								JavaScriptSerializer jss = new JavaScriptSerializer();
								try
								{
									json = GetStringFromURL(wc, "http://beestuff.pythonanywhere.com/api");
									audbResponse = jss.Deserialize<ApiEndpointResponse>(json);
								}
								catch (Exception e)
                                {
									Console.WriteLine("Error fetching AUDB endpoints:");
									Console.WriteLine(e);
									break;
                                }
								ApiEndpointResponse.EndpointData newmodEndpoint = null;
								foreach (ApiEndpointResponse.EndpointData endpointData in audbResponse.endpoints)
								{
									if (endpointData.path == "/audb/api/mods/new")
									{
										newmodEndpoint = endpointData;
									}
								}
								if (newmodEndpoint == null)
								{
									Console.WriteLine("Mod creation endpoint couldn't be found, sorry!");
									Console.WriteLine("This is probably an internal problem with the API, or you have an outdated version of AUDBTool.");
								}
								else
								{
									DoAuthstring(wc, rsa_csp, currentKey, jss);
									Dictionary<string, object> builtResponse = new Dictionary<string, object>();
									foreach (string key in ((Dictionary<string, object>)newmodEndpoint.request).Keys)
									{
										if (((Dictionary<string, object>)newmodEndpoint.request)[key] is string && (string)((Dictionary<string, object>)newmodEndpoint.request)[key] == "string")
										{
											Console.Write("Please enter " + key + ": ");
											builtResponse[key] = Console.ReadLine();
										}
										else if (((Dictionary<string, object>)newmodEndpoint.request)[key] is string && (string)((Dictionary<string, object>)newmodEndpoint.request)[key] == "bool")
										{
											builtResponse[key] = PromptBinary(key + "?");
										}
										else
										{
											builtResponse[key] = null;
										}
									}
									Console.WriteLine("\n-----------------------------------------------\n");
									Console.WriteLine(jss.Serialize(builtResponse));
									Console.WriteLine("\n-----------------------------------------------\n");
									Console.WriteLine("NOTE: \"filename\" in this context means name of the file returned on a manual download; irrelevant for AutoUpdate.");
									if (PromptBinary("Are you sure you want to create this mod entry on AUDB?"))
                                    {
										string updateURL = PostStringToURL(wc, "http://beestuff.pythonanywhere.com/audb/api/mods/new", jss.Serialize(builtResponse));
										Console.WriteLine("// ------------------------------------------------");
										Console.WriteLine("// Code for AutoUpdate support");
										Console.WriteLine("// Should be put in the main PartialityMod class.");
										Console.WriteLine("// Comments are, obviously, optional.");
										Console.WriteLine();
										Console.WriteLine("// Update URL - don't touch!");
										Console.WriteLine("// You can go to this in a browser (it's safe), but you might not understand the result.");
										Console.WriteLine("// This URL is specific to this mod, and identifies it on AUDB.");
										Console.WriteLine("public string updateURL = \"{0}\";", updateURL);
										Console.WriteLine("// Version - increase this by 1 when you upload a new version of the mod.");
										Console.WriteLine("// The first upload should be with version 0, the next version 1, the next version 2, etc.");
										Console.WriteLine("// If you ever lose track of the version you're meant to be using, ask Pastebin.");
										Console.WriteLine("public int version = 0;");
										Console.WriteLine("// Public key in base64 - don't touch!");
										Console.WriteLine("public string keyE = \"{0}\";", Convert.ToBase64String(currentKey.Exponent));
										Console.WriteLine("public string keyN = \"{0}\";", Convert.ToBase64String(currentKey.Modulus));
										Console.WriteLine("// ------------------------------------------------");
										Console.WriteLine("If you include the code above and upload the mod, it will immediately be able to support AutoUpdate (select and press Enter to copy to clipboard).");
										if (PromptBinary("\nWould you like to upload initial version of the mod now?"))
										{
											QueuedAction = "uploadmod";
											string[] array = updateURL.Split('/');
											currModID = int.Parse(array[array.Length - 1]);
										}

									}
									
								}
								break;
							}
						case "uploadmod":
							{
								string stringFromURL3;
								JavaScriptSerializer jss = new JavaScriptSerializer();
								KeyStructure currKeyStruct;
								try
								{
									stringFromURL3 = GetStringFromURL(wc, "http://beestuff.pythonanywhere.com/keydb/api/keys/" + currUID.ToString());
									currKeyStruct = jss.Deserialize<KeyStructure>(stringFromURL3);
								}
								catch (Exception e)
								{
									Console.WriteLine($"Error while fetching keystruct for user {currUID} from AUDB:");
									Console.WriteLine(e);
									break;
								}
								for (int i = 0; i < currKeyStruct.audb.mods.Count; i++)
								{
									Console.WriteLine("Mod ID {0}: {1}", i, currKeyStruct.audb.mods[i].metadata.name);
								}
								if (currModID == -1)
								{
									currModID = PromptInt("Please enter the mod ID you want to upload.");
								}
								Console.Write("Please enter local filepath of the mod to upload:");
								string locPath = Console.ReadLine();
								try
								{
									byte[] buffer = File.ReadAllBytes(locPath);
									byte[] inArray = rsa_csp.SignData(buffer, "SHA512");
									string value = Convert.ToBase64String(inArray);
									DoAuthstring(wc, rsa_csp, currentKey, jss);
									wc.Headers.Set("modsig", value);
									PostFileToURL(wc, "http://beestuff.pythonanywhere.com/audb/api/mods/" + currModID.ToString() + "/upload", locPath);
								}
								catch (Exception e)
								{
									Console.WriteLine("Error uploading mod:");
									Console.WriteLine(e);
								}
								currModID = -1;
								break;
							}
					case "uploadthumb":
					{
						JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
						string stringFromURL4 = GetStringFromURL(wc, "http://beestuff.pythonanywhere.com/keydb/api/keys/" + currUID.ToString());
						KeyStructure keyStructure2 = javaScriptSerializer.Deserialize<KeyStructure>(stringFromURL4);
						for (int i = 0; i < keyStructure2.audb.mods.Count; i++)
						{
							Console.WriteLine("Mod ID {0}: {1}", i, keyStructure2.audb.mods[i].metadata.name);
						}
						if (currModID == -1)
						{
							currModID = PromptInt("Please enter the mod ID you want to upload the thumbnail (image) of.");
						}
						Console.Write("Please enter the filename of the image you want to upload: ");
						string text9 = Console.ReadLine();
						byte[] array2 = File.ReadAllBytes(text9);
						DoAuthstring(wc, rsa_csp, currentKey, javaScriptSerializer);
						PostFileToURL(wc, "http://beestuff.pythonanywhere.com/audb/api/mods/" + currModID.ToString() + "/thumb", text9);
						currModID = -1;
						break;
					}
					case "sign":
					{
						Console.WriteLine("WARNING: Signing data is the only method of authorization for AUDB. ONLY do this if Pastebin has manually asked you to, or you know EXACTLY what you are doing! NO EXCEPTIONS!");
						Console.Write("anyway now that we've done that warning, what do you actually want to sign? (there's no confirmation so input it right the first time please) ");
						string s = Console.ReadLine();
						byte[] inArray2 = rsa_csp.SignData(Encoding.ASCII.GetBytes(s), "SHA512");
						string str2 = Convert.ToBase64String(inArray2);
						Console.WriteLine("signature: " + str2);
						break;
					}
					case "versions":
					{
						JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
						string stringFromURL5 = GetStringFromURL(wc, "http://beestuff.pythonanywhere.com/keydb/api/keys");
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
						QueuedAction = null;
					}
				}
				catch (WebException ex)
				{
					Console.WriteLine("ERROR RESPONSE:");
					Console.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
				}
			}
		}

        public static string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/0/7";
		public static int version = 14;
		public static string keyE = "AQAB";
		public static string keyN = "yu7XMmICrzuavyZRGWoknFIbJX4N4zh3mFPOyfzmQkil2axVIyWx5ogCdQ3OTdSZ0xpQ3yiZ7zqbguLu+UWZMfLOBKQZOs52A9OyzeYm7iMALmcLWo6OdndcMc1Uc4ZdVtK1CRoPeUVUhdBfk2xwjx+CvZUlQZ26N1MZVV0nq54IOEJzC9qQnVNgeeHxO1lRUTdg5ZyYb7I2BhHfpDWyTvUp6d5m6+HPKoalC4OZSfmIjRAi5UVDXNRWn05zeT+3BJ2GbKttwvoEa6zrkVuFfOOe9eOAWO3thXmq9vJLeF36xCYbUJMkGR2M5kDySfvoC7pzbzyZ204rXYpxxXyWPP5CaaZFP93iprZXlSO3XfIWwws+R1QHB6bv5chKxTZmy/Imo4M3kNLo5B2NR/ZPWbJqjew3ytj0A+2j/RVwV9CIwPlN4P50uwFm+Mr0OF2GZ6vU0s/WM7rE78+8Wwbgcw6rTReKhVezkCCtOdPkBIOYv3qmLK2S71NPN2ulhMHD9oj4t0uidgz8pNGtmygHAm45m2zeJOhs5Q/YDsTv5P7xD19yfVcn5uHpSzRIJwH5/DU1+aiSAIRMpwhF4XTUw73+pBujdghZdbdqe2CL1juw7XCa+XfJNtsUYrg+jPaCEUsbMuNxdFbvS0Jleiu3C8KPNKDQaZ7QQMnEJXeusdU=";
	}
}
