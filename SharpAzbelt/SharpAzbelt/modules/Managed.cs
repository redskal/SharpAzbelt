using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SharpAzbelt
{
    class Managed
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<String> runManaged()
        {
            string outText = "[i] Managed Identity:\n";
            string outFormat = "[+]    {0,-25} : {1}\n";

            // try grabbing the usual metadata output
            string resMetadata = await GetJsonFromHttp("http://169.254.169.254/metadata/instance?api-version=2021-12-13");
            if (resMetadata != "")
            {
                outText += string.Format(outFormat, "Metadata", null);
                outText += resMetadata + "\n";
            }
            else
            {
                outText += "[-]    No metadata found\n";
            }
            
            string resToken = await GetJsonFromHttp("http://169.254.169.254/metadata/identity/oauth2/token?api-version=2021-12-13&resource=https://management.azure.com");
            if (resToken != "")
            {
                outText += string.Format(outFormat, "Token", null);
                outText += resToken + "\n";
            }
            else
            {
                outText += "[-]    No tokens found\n";
            }

            return outText + "\n";
        }

        private static async Task<String> GetJsonFromHttp(string Uri)
        {
            string outText = "";
            try
            {
                // try grabbing the usual metadata output
                var msg = new HttpRequestMessage(HttpMethod.Get, Uri);
                //set our headers - trying to mimic MS Edge for the sneaky beakies
                msg.Headers.Add("Content-Type", "application/json");
                msg.Headers.Add("Metadata", "true");
                // https://learn.microsoft.com/en-us/microsoft-edge/web-platform/user-agent-guidance
                msg.Headers.Add("User-Agent", "Mozilla / 5.0(Windows NT 10.0; Win64; x64) AppleWebKit / 537.36(KHTML, like Gecko) Chrome / 90.0.4430.85 Safari / 537.36 Edg / 90.0.818.46");
                msg.Headers.Add("Sec-CH-UA", "\"Chromium\";v=\"92\", \"Microsoft Edge\";v=\"92\", \"Placeholder; Browser Brand\";v=\"99\"");
                msg.Headers.Add("Sec-CH-UA-Mobile", "?0");
                msg.Headers.Add("Sec-CH-UA-Platform", "\"Windows\"");

                var response = await client.SendAsync(msg);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    outText += jsonResponse + "\n";
                }
            }
            catch (Exception)
            {
                // do nowt
            }

            return outText;
        }
    }
}
