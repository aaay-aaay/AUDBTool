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
    internal static class ConsoleUtils
    {
        internal static string Prompt(string prompt, string[] options)
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
                Console.Write("]\n> ");
                string text = Console.ReadLine();
                if (!options.Contains(text))
                {
                    result = Prompt("That was not a valid choice. " + prompt, options);
                }
                else
                {
                    result = text;
                }
            }
            return result;
        }


        internal static bool PromptBinary(string message)
        {
            Console.WriteLine($"{message} (y/n)");
            Console.Write("> ");
            for (; ; )
            {
                var r = Console.ReadLine();
                switch (r)
                {
                    case "y": return true;
                    case "n": return false;
                    default:
                        Console.WriteLine("This is not a valid choice!");
                        break;
                }
            }
        }
        internal static int PromptInt(string prompt)
        {
            Console.Write("{0}\n> ", prompt);
            string s = Console.ReadLine();
            int result;
            for (; ; )
            {
                if (int.TryParse(s, out result)) return result;
                else Console.WriteLine("This is not a valid number.");
            }
        }
    }
}