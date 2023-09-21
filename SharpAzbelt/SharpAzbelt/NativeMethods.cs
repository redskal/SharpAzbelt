using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpAzbelt
{
    class NativeMethods
    {
        // https://pinvoke.net/default.aspx/Enums/CryptProtectFlags.html
        [Flags]
        public enum CryptProtectFlags
        {
            // for remote-access situations where ui is not an option
            // if UI was specified on protect or unprotect operation, the call
            // will fail and GetLastError() will indicate ERROR_PASSWORD_RESTRICTION
            CRYPTPROTECT_UI_FORBIDDEN = 0x1,

            // per machine protected data -- any user on machine where CryptProtectData
            // took place may CryptUnprotectData
            CRYPTPROTECT_LOCAL_MACHINE = 0x4,

            // force credential synchronize during CryptProtectData()
            // Synchronize is only operation that occurs during this operation
            CRYPTPROTECT_CRED_SYNC = 0x8,

            // Generate an Audit on protect and unprotect operations
            CRYPTPROTECT_AUDIT = 0x10,

            // Protect data with a non-recoverable key
            CRYPTPROTECT_NO_RECOVERY = 0x20,

            // Verify the protection of a protected blob
            CRYPTPROTECT_VERIFY_PROTECTION = 0x40
        }

        // https://pinvoke.net/default.aspx/Enums/CryptProtectPromptFlags.html
        [Flags]
        public enum CryptProtectPromptFlags
        {
            // prompt on unprotect
            CRYPTPROTECT_PROMPT_ON_UNPROTECT = 0x1,

            // prompt on protect
            CRYPTPROTECT_PROMPT_ON_PROTECT = 0x2
        }

        // https://pinvoke.net/default.aspx/Structures/CRYPTPROTECT_PROMPTSTRUCT.html
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CRYPTPROTECT_PROMPTSTRUCT
        {
            public int cbSize;
            public CryptProtectPromptFlags dwPromptFlags;
            public IntPtr hwndApp;
            public String szPrompt;
        }

        // https://pinvoke.net/default.aspx/Structures/DATA_BLOB.html
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DATA_BLOB
        {
            public int cbData;
            public IntPtr pbData;
        }

        // http://pinvoke.net/default.aspx/advapi32.CredRead
        public enum CRED_TYPE : uint
        {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            GENERIC_CERTIFICATE = 5,
            DOMAIN_EXTENDED = 6,
            MAXIMUM = 7,      // Maximum supported cred type
            MAXIMUM_EX = (MAXIMUM + 1000)  // Allow new applications to run on old OSes
        }

        // http://pinvoke.net/default.aspx/advapi32.CredRead
        public enum CRED_PERSIST : uint
        {
            SESSION = 1,
            LOCAL_MACHINE = 2,
            ENTERPRISE = 3
        }

        // http://pinvoke.net/default.aspx/advapi32.CredRead
        // eg: NATIVECREDENTIAL lRawCredential = (NATIVECREDENTIAL)Marshal.PtrToStructure(pNativeData, typeof(NATIVECREDENTIAL));
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public class NATIVECREDENTIAL
        {
            public UInt32 Flags;
            public CRED_TYPE Type;
            public string TargetName;
            public string Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public UInt32 CredentialBlobSize;
            public IntPtr CredentialBlob;
            public CRED_PERSIST Persist;
            public UInt32 AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }

        [DllImport("advapi32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool CredEnumerateW(
            string filter,
            int flag,
            out int count,
            out IntPtr pCredentials
        );

        // http://pinvoke.net/default.aspx/crypt32.CryptUnprotectData
        [DllImport("Crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CryptUnprotectData(
            ref DATA_BLOB pDataIn,
            string szDataDescr,
            ref DATA_BLOB pOptionalEntropy,
            IntPtr pvReserved,
            ref CRYPTPROTECT_PROMPTSTRUCT pPromptStruct,
            CryptProtectFlags dwFlags,
            ref DATA_BLOB pDataOut
        );

        // https://learn.microsoft.com/en-us/windows/win32/api/lmjoin/ne-lmjoin-dsreg_join_type
        public enum DSREG_JOIN_TYPE
        {
            DSREG_UNKNOWN_JOIN,
            DSREG_DEVICE_JOIN,
            DSREG_WORKPLACE_JOIN
        }

        // https://learn.microsoft.com/en-us/windows/win32/api/lmjoin/ns-lmjoin-dsreg_user_info
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DSREG_USER_INFO
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string UserEmail;
            [MarshalAs(UnmanagedType.LPWStr)] public string UserKeyId;
            [MarshalAs(UnmanagedType.LPWStr)] public string UserKeyName;
        }

        // https://learn.microsoft.com/en-us/windows/win32/api/wincrypt/ns-wincrypt-cert_context
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CERT_CONTEXT
        {
            public uint CertEncodingType;
            public byte pbCertEncoded;
            public uint cbCertEncoded;
            public IntPtr pCertInfo;
            public IntPtr hCertStore;
        }

        // https://learn.microsoft.com/en-us/windows/win32/api/lmjoin/ns-lmjoin-dsreg_join_info
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DSREG_JOIN_INFO
        {
            public int joinType;
            public IntPtr pJoinCertificate;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszDeviceId;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszIdpDomain;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszTenantId;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszJoinUserEmail;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszTenantDisplayName;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszMdmEnrollmentUrl;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszMdmTermsOfUseUrl;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszMdmComplianceUrl;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszUserSettingSyncUrl;
            public IntPtr pUserInfo;
        }

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int NetGetAadJoinInformation(
            string pcszTenantId,
            out IntPtr ppJoinInfo
        );

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern void NetFreeAadJoinInformation(
            IntPtr pJoinInfo
        );

        public static string cryptUnprotectData(byte[] data)
        {
            string outText = "";
            try
            {
                IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(byte)) * data.Length);
                Marshal.Copy(data, 0, p, data.Length);

                var pDataIn = new DATA_BLOB
                {
                    cbData = data.Length,
                    pbData = p
                };
                var pOptionalEntropy = new DATA_BLOB();
                var pPromptStruct = new CRYPTPROTECT_PROMPTSTRUCT();
                var pDataOut = new DATA_BLOB();

                bool ret = CryptUnprotectData(
                    ref pDataIn,
                    null,
                    ref pOptionalEntropy,
                    IntPtr.Zero,
                    ref pPromptStruct,
                    0,
                    ref pDataOut);

                if (ret)
                {
                    var decrypted = new byte[pDataOut.cbData];
                    Marshal.Copy(pDataOut.pbData, decrypted, 0, pDataOut.cbData);
                    outText = Encoding.Default.GetString(decrypted);
                }

                Marshal.FreeHGlobal(p);
            }
            catch (Exception)
            {
                // ...
            }

            return outText;
        }
    }
}
