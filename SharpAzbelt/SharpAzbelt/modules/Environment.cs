using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace SharpAzbelt
{
    class Environment
    {
        public static string runEnv()
        {
            // do stuff
            string outText = "[i] Environment:\n";
            string outFormat = "[+]    {0,-25} : {1}\n";

            List<String> envs = new List<String> { 
                        "AZURE_CLIENT_ID",
                        "AZURE_TENANT_ID",
                        "AZURE_USERNAME",
                        "AZURE_PASSWORD",
                        "AZURE_CLIENT_SECRET",
                        "AZURE_CLIENT_CERTIFICATE_PATH",
                        "AZURE_CLIENT_CERTIFICATE_PASSWORD",
                        "AZURE_POD_IDENTITY_AUTHORITY_HOST",
                        "IDENTITY_ENDPOINT",
                        "IDENTITY_HEADER",
                        "IDENTITY_SERVER_THUMBPRINT",
                        "IMDS_ENDPOINT",
                        "MSI_ENDPOINT",
                        "MSI_SECRET",
                        "AZURE_AUTHORITY_HOST",
                        "AZURE_IDENTITY_DISABLE_MULTITENANTAUTH",
                        "AZURE_REGIONAL_AUTHORITY_NAME",
                        "AZURE_FEDERATED_TOKEN_FILE",
                        "ONEDRIVE"
                    };

            foreach (string s in envs)
            {
                string tmp = System.Environment.GetEnvironmentVariable(s);
                if (tmp != null)
                    outText += string.Format(outFormat, s, tmp);
            }

            return outText + "\n";
        }
    }
}
