/*
 * A (bad) C# port of Leron Gray's Azbelt - https://github.com/daddycocoaman/azbelt/
 * 
 * The original version is a collection of techniques put into one Nim-based DLL.
 * I've had various issues with it, whereby my Sliver implants die. Now it can be
 * run with execute-assembly in-process, and doesn't break anything (so far).
 */
using System;
using System.Threading.Tasks;

namespace SharpAzbelt
{
    class Program
    {
        private static string banner = @"
   _____ __                     ___        __         ____ 
  / ___// /_  ____ __________  /   |____  / /_  ___  / / /_
  \__ \/ __ \/ __ `/ ___/ __ \/ /| /_  / / __ \/ _ \/ / __/
 ___/ / / / / /_/ / /  / /_/ / ___ |/ /_/ /_/ /  __/ / /_  
/____/_/ /_/\__,_/_/  / .___/_/  |_/___/_.___/\___/_/\__/  
                     /_/                                   
";

        private static string usage = @"
[!] You've taken a wrong turn...
[.]
[i] Options:
[i]     all        - all checks
[i]     aadjoin    - check for AzureAD-joined information
[i]     credman    - dump CredentialManager entries for user
[i]     env        - check Azure-related environment variables
[i]     tbres      - loot any TokenBroker caches
[i]     managed    - search managed identity endpoints (use on VM, VMSS, WVD, etc)
[i]     msal       - loot the MSAL token caches
[i]     sso        - get signed PRT cookie from AAD joined hosts
";
        static async Task Main(string[] args)
        {
            Console.WriteLine(banner);
            if (args.Length == 0)
            {
                Console.WriteLine(usage);
                goto Exit;
            }

            string outText = "";
            switch (args[0].ToLower())
            {
                case string aad when aad.Contains("aadjoin"):
                    outText = Aadjoin.runAadJoin();
                    Console.WriteLine(outText);
                    break;
                case string cred when cred.Contains("credman"):
                    outText = Credman.runCredman();
                    Console.WriteLine(outText);
                    break;
                case string env when env.Contains("env"):
                    outText = Environment.runEnv();
                    Console.WriteLine(outText);
                    break;
                case string managed when managed.Contains("managed"):
                    outText = await Managed.runManaged();
                    Console.WriteLine(outText);
                    break;
                case string msal when msal.Contains("msal"):
                    outText = Msal.runMsal();
                    Console.WriteLine(outText);
                    break;
                case string tbres when tbres.Contains("tbres"):
                    outText = Tbres.runTbres();
                    Console.WriteLine(outText);
                    break;
                case string sso when sso.Contains("sso"):
                    outText = await Sso.runSso();
                    Console.WriteLine(outText);
                    break;
                case string all when all.Contains("all"):
                    outText = Aadjoin.runAadJoin();
                    Console.WriteLine(outText);
                    outText = await Managed.runManaged();
                    Console.WriteLine(outText);
                    outText = Environment.runEnv();
                    Console.WriteLine(outText);
                    outText = await Sso.runSso();
                    Console.WriteLine(outText);
                    outText = Tbres.runTbres();
                    Console.WriteLine(outText);
                    outText = Msal.runMsal();
                    Console.WriteLine(outText);
                    outText = Credman.runCredman();
                    Console.WriteLine(outText);
                    break;
                default:
                    // error
                    break;
            }

        Exit:
            return;
        }
    }
}
