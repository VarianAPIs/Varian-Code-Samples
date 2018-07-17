using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Extensions
{
    public static class SecureStringExtensions
    {
        public static string GetNonSecureString(this SecureString ss)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(ss);
                return Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }

        public static SecureString GetSecureString(this string s)
        {
            SecureString ss = new SecureString();
            for (int i = 0; i < s.Length; i++)
            {
                ss.AppendChar(s[i]);
            }
            return ss;
        }
    }
}
