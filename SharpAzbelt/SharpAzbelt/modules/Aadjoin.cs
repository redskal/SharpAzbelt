/*
 * Based on https://github.com/sliverarmory/azbelt/blob/main/src/azbelt/modules/aadjoin.nim
 * 
 * Used for reference and guidance: https://gist.github.com/benpturner/c7376718558bb118111c7cad651a25ce
 */
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SharpAzbelt
{
    class Aadjoin
    {
        public static string runAadJoin()
        {
            string outText = "[i] AAD Join:\n";
            string outFormat = "[+]    {0,-25} : {1}\n";
            string pcszTenantId = null;
            var ptrJoinInfo = IntPtr.Zero;
            var ptrUserInfo = IntPtr.Zero;
            NativeMethods.DSREG_JOIN_INFO joinInfo = new NativeMethods.DSREG_JOIN_INFO();

            NativeMethods.NetFreeAadJoinInformation(IntPtr.Zero);
            var retValue = NativeMethods.NetGetAadJoinInformation(pcszTenantId, out ptrJoinInfo);

            // Did it fail?
            if (retValue != 0)
            {
                outText += "[-]    Not Azure joined\n";
                goto Exit;
            }
            
            // enumerate the DSREG_JOIN_INFO parts
            try
            {
                NativeMethods.DSREG_JOIN_INFO ptrJoinInfoObject = new NativeMethods.DSREG_JOIN_INFO();
                joinInfo = (NativeMethods.DSREG_JOIN_INFO)Marshal.PtrToStructure(ptrJoinInfo, (System.Type)ptrJoinInfoObject.GetType());
                
                switch ((NativeMethods.DSREG_JOIN_TYPE)joinInfo.joinType)
                {
                    case (NativeMethods.DSREG_JOIN_TYPE.DSREG_DEVICE_JOIN):
                        {
                            outText += string.Format(outFormat, "Join Type", "AzureAD");
                            break;
                        }
                    case (NativeMethods.DSREG_JOIN_TYPE.DSREG_UNKNOWN_JOIN):
                        {
                            outText += string.Format(outFormat, "Join Type", "Device is not AzureAD joined");
                            break;
                        }
                    case (NativeMethods.DSREG_JOIN_TYPE.DSREG_WORKPLACE_JOIN):
                        {
                            outText += string.Format(outFormat, "Join Type", "Workplace");
                            break;
                        }
                }

                outText += string.Format(outFormat, "Username", joinInfo.pszJoinUserEmail);
                outText += string.Format(outFormat, "Tenant ID", joinInfo.pszTenantId);
                outText += string.Format(outFormat, "Tenant Display Name", joinInfo.pszTenantDisplayName);
                outText += string.Format(outFormat, "Device ID", joinInfo.pszDeviceId);
                outText += string.Format(outFormat, "IDP Domain", joinInfo.pszIdpDomain);
                outText += string.Format(outFormat, "MDM Enrollment URL", joinInfo.pszMdmEnrollmentUrl);
            }
            catch (Exception)
            {
                outText += "[!]    Failed to enumerate DSREG_JOIN_INFO\n";
            }

            // enumerate the UserInfo struct
            if (joinInfo.pUserInfo != IntPtr.Zero)
            {
                try
                {
                    ptrUserInfo = joinInfo.pUserInfo;
                    NativeMethods.DSREG_USER_INFO ptrUserInfoObject = new NativeMethods.DSREG_USER_INFO();
                    NativeMethods.DSREG_USER_INFO userInfo = (NativeMethods.DSREG_USER_INFO)Marshal.PtrToStructure(ptrUserInfo, (System.Type)ptrUserInfoObject.GetType());

                    FieldInfo[] fi = typeof(NativeMethods.DSREG_USER_INFO).GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (FieldInfo info in fi)
                    {
                        var x = info.GetValue(userInfo);
                        outText += $" [+]   {info.Name,-25} : {x}\n";
                    }
                }
                catch (Exception)
                {
                    outText += "[!]    Failed to enumerate DSREG_JOIN_INFO.DSREG_USER_INFO\n";
                }
            }

            // clean up
            try
            {
                if (ptrJoinInfo != IntPtr.Zero)
                    Marshal.Release(ptrJoinInfo);
                if (ptrUserInfo != IntPtr.Zero)
                    Marshal.Release(ptrUserInfo);
            }
            catch (Exception)
            {
                outText += "[!]    Failed to clean up DSREG_USER_INFO\n";
            }

        Exit:
            return outText + "\n";
        }
    }
}
