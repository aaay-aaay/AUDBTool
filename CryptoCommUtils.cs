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

namespace PastebinMachine.AutoUpdate.CryptoTool
{
    internal static class CryptoCommUtils
    {
        internal static void CheckUpdate(WebClient client)
        {
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
            string stringFromURL = GetStringFromURL(client, AUDBTool.updateURL);
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
        internal static void DoAuthstring(WebClient client, RSACryptoServiceProvider rsa, RSAParameters rsaParams, JavaScriptSerializer jss)
        {
            client.Headers.Set("Authorization-E", Convert.ToBase64String(rsaParams.Exponent));
            client.Headers.Set("Authorization-N", Convert.ToBase64String(rsaParams.Modulus));
            string stringFromURL = GetStringFromURL(client, "http://beestuff.pythonanywhere.com/keydb/api/auth/get");
            string authstring = jss.Deserialize<AuthgetResponse>(stringFromURL).authstring;
            string text = "authsign:" + authstring;
            Console.WriteLine("signing " + text);
            byte[] bytes = Encoding.ASCII.GetBytes(text);
            byte[] inArray = rsa.SignData(bytes, "SHA512");
            string value = Convert.ToBase64String(inArray);
            client.Headers.Set("authstring", authstring);
            client.Headers.Set("sig", value);
        }
        internal static string DoFormat(string url)
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
        internal static int GetKeyID(RSAParameters rsaParams, WebClient client)
        {
            JavaScriptSerializer jsonSer = new JavaScriptSerializer();
            try
            {
                string json = GetStringFromURL(client, "http://beestuff.pythonanywhere.com/keydb/api/keys");
                List<KeyStructure> list = jsonSer.Deserialize<List<KeyStructure>>(json);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].e == Convert.ToBase64String(rsaParams.Exponent) && list[i].n == Convert.ToBase64String(rsaParams.Modulus))
                    {
                        Console.WriteLine("Using key {0}", i);
                        return i;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error encountered while checking key ID:");
                Console.WriteLine(e);
            }

            return -1;
        }
        internal static string GetStringFromURL(WebClient client, string url)
        {
            Stream stream = client.OpenRead(url);
            StreamReader streamReader = new StreamReader(stream);
            string result = streamReader.ReadToEnd();
            stream.Close();
            streamReader.Close();
            return result;
        }
        internal static string KeySaveString(RSAParameters rsaParams)
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
        internal static byte[] PostFileToURL(WebClient client, string url, string file)
        {
            return client.UploadFile(url, file);
        }
        internal static string PostStringToURL(WebClient client, string url, string data)
        {
            client.Headers[HttpRequestHeader.ContentType] = "application/json";
            return client.UploadString(url, data);
        }
        internal static RSAParameters ReadKey(RSAParameters rsaParams, string filename)
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
        internal static void SaveKey(RSAParameters rsaParams, string filename)
        {
            string contents = KeySaveString(rsaParams);
            File.WriteAllText(filename, contents);
        }
    }
}