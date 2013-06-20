//===============================================================================
// Microsoft patterns & practices
// Enterprise Library 6 Samples
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace CustomSinkExtension
{
    internal static class CredentialManager
    {
        public static NetworkCredential GetCredentials(string target)
        {
            Credential credential;
            bool bSuccess = NativeMethods.CredRead(target, NativeMethods.CRED_TYPE.GENERIC, 0, out credential);

            if (!bSuccess)
            {
                throw new SecurityException(string.Format(CultureInfo.CurrentCulture, "Credentials not found for '{0}'", target));
            }

            return new NetworkCredential(credential.UserName, credential.Password);
        }

        internal static class NativeMethods
        {
            [DllImport("advapi32.dll", SetLastError = true, EntryPoint = "CredReadW", CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CredRead(string target, CRED_TYPE type, int reservedFlag,
                [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(CredentialMarshaler))] out Credential credential);

            [DllImport("Advapi32.dll", EntryPoint = "CredFree", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CredFree(IntPtr cred);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            internal struct CREDENTIAL_STRUCT
            {
                public UInt32 flags;
                public UInt32 type;
                public string targetName;
                public string comment;
                public System.Runtime.InteropServices.ComTypes.FILETIME lastWritten;
                public UInt32 credentialBlobSize;
                public IntPtr credentialBlob;
                public UInt32 persist;
                public UInt32 attributeCount;
                public IntPtr credAttribute;
                public string targetAlias;
                public string userName;
            }

            internal enum CRED_TYPE : int
            {
                GENERIC = 1,
                DOMAIN_PASSWORD = 2,
                DOMAIN_CERTIFICATE = 3,
                DOMAIN_VISIBLE_PASSWORD = 4,
                MAXIMUM = 5
            }

            internal enum CRED_PERSIST : uint
            {
                SESSION = 1,
                LOCAL_MACHINE = 2,
                ENTERPRISE = 3
            }
        }

        private sealed class CredentialMarshaler : ICustomMarshaler
        {
            private static CredentialMarshaler _instance;

            public void CleanUpManagedData(object ManagedObj)
            {
                // Nothing to do since all data can be garbage collected.
            }

            public void CleanUpNativeData(IntPtr pNativeData)
            {
                if (pNativeData == IntPtr.Zero)
                {
                    return;
                }
                NativeMethods.CredFree(pNativeData);
            }

            public int GetNativeDataSize()
            {
                throw new NotImplementedException("The method or operation is not implemented.");
            }

            public IntPtr MarshalManagedToNative(object obj)
            {
                throw new NotImplementedException("Not implemented yet");
            }

            public object MarshalNativeToManaged(IntPtr pNativeData)
            {
                if (pNativeData == IntPtr.Zero)
                {
                    return null;
                }
                return new Credential((NativeMethods.CREDENTIAL_STRUCT)Marshal.PtrToStructure(pNativeData, typeof(NativeMethods.CREDENTIAL_STRUCT)));
            }


            public static ICustomMarshaler GetInstance(string cookie)
            {
                if (null == _instance)
                    _instance = new CredentialMarshaler();
                return _instance;
            }
        }

        internal sealed class Credential
        {
            private SecureString _userName;
            private SecureString _password;

            internal Credential(NativeMethods.CREDENTIAL_STRUCT cred)
            {
                _userName = ConvertToSecureString(cred.userName);
                int size = (int)cred.credentialBlobSize;
                if (size != 0)
                {
                    byte[] bpassword = new byte[size];
                    Marshal.Copy(cred.credentialBlob, bpassword, 0, size);
                    _password = ConvertToSecureString(Encoding.Unicode.GetString(bpassword));
                }
                else
                {
                    _password = ConvertToSecureString(String.Empty);
                }
            }

            public string UserName
            {
                get { return ConvertToUnsecureString(_userName); }
            }

            public SecureString Password
            {
                get { return _password; }
            }

            /// <summary>
            /// This converts a SecureString password to plain text
            /// </summary>
            /// <param name="securePassword">SecureString password</param>
            /// <returns>plain text password</returns>
            private string ConvertToUnsecureString(SecureString secret)
            {
                if (secret == null)
                    return string.Empty;

                IntPtr unmanagedString = IntPtr.Zero;
                try
                {
                    unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secret);
                    return Marshal.PtrToStringUni(unmanagedString);
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
                }
            }

            /// <summary>
            /// This converts a string to SecureString
            /// </summary>
            /// <param name="password">plain text password</param>
            /// <returns>SecureString password</returns>
            private SecureString ConvertToSecureString(string secret)
            {
                if (string.IsNullOrEmpty(secret))
                    return null;

                SecureString securePassword = new SecureString();
                char[] passwordChars = secret.ToCharArray();
                foreach (char pwdChar in passwordChars)
                {
                    securePassword.AppendChar(pwdChar);
                }
                securePassword.MakeReadOnly();
                return securePassword;
            }
        }
    }
}
