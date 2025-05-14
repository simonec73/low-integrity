using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace LowIntegrityServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Integrity Level: " + GetProcessIntegrityLevelDescription());
            NamedPipeServerStream pipeServer = new NamedPipeServerStream(@"LowIntegrityNamedPipe",
                PipeDirection.InOut, 100, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0);
            pipeServer.BeginWaitForConnection(HandleConnection, pipeServer);

            Console.WriteLine("Waiting...");
            Console.ReadLine();
        }

        private static void HandleConnection(IAsyncResult ar)
        {
            Console.WriteLine("Connection established.");

            NamedPipeServerStream pipeServer = ar.AsyncState as NamedPipeServerStream;

            if (pipeServer != null)
            {
                pipeServer.EndWaitForConnection(ar);

                if (pipeServer.IsConnected)
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes("This is the output from the Server!");
                    pipeServer.Write(bytes, 0, bytes.Length);
                    pipeServer.Flush();

                    Console.WriteLine("Message sent.");

                    byte[] buffer = new byte[100];
                    bool close = false;

                    do
                    {
                        int len = pipeServer.Read(buffer, 0, 100);

                        string text = System.Text.Encoding.UTF8.GetString(buffer, 0, len);

                        if (text == "Close")
                        {
                            close = true;
                        } else if (text == "GetAnotherString")
                        {
                            bytes = System.Text.Encoding.UTF8.GetBytes("This is another string!");
                            pipeServer.Write(bytes, 0, bytes.Length);
                            pipeServer.Flush();

                            Console.WriteLine("Another message sent.");
                        }
                    } while (!close);

                    pipeServer.Close();
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("Client is not connected!");
                }
            }
        }

        #region Helper Functions related to Process Integrity Level
        /// <summary>
        /// The function gets the integrity level of the current process. Integrity 
        /// level is only available on Windows Vista and newer operating systems, thus 
        /// GetProcessIntegrityLevel throws a C++ exception if it is called on systems 
        /// prior to Windows Vista.
        /// </summary>
        /// <returns>
        /// Returns the integrity level of the current process. It is usually one of 
        /// these values:
        /// 
        ///    SECURITY_MANDATORY_UNTRUSTED_RID - means untrusted level. It is used 
        ///    by processes started by the Anonymous group. Blocks most write access.
        ///    (SID: S-1-16-0x0)
        ///    
        ///    SECURITY_MANDATORY_LOW_RID - means low integrity level. It is used by
        ///    Protected Mode Internet Explorer. Blocks write acess to most objects 
        ///    (such as files and registry keys) on the system. (SID: S-1-16-0x1000)
        /// 
        ///    SECURITY_MANDATORY_MEDIUM_RID - means medium integrity level. It is 
        ///    used by normal applications being launched while UAC is enabled. 
        ///    (SID: S-1-16-0x2000)
        ///    
        ///    SECURITY_MANDATORY_HIGH_RID - means high integrity level. It is used 
        ///    by administrative applications launched through elevation when UAC is 
        ///    enabled, or normal applications if UAC is disabled and the user is an 
        ///    administrator. (SID: S-1-16-0x3000)
        ///    
        ///    SECURITY_MANDATORY_SYSTEM_RID - means system integrity level. It is 
        ///    used by services and other system-level applications (such as Wininit, 
        ///    Winlogon, Smss, etc.)  (SID: S-1-16-0x4000)
        /// 
        /// </returns>
        /// <exception cref="System.ComponentModel.Win32Exception">
        /// When any native Windows API call fails, the function throws a Win32Exception 
        /// with the last error code.
        /// </exception>
        internal static int GetProcessIntegrityLevel()
        {
            int IL = -1;
            SafeTokenHandle hToken = null;
            int cbTokenIL = 0;
            IntPtr pTokenIL = IntPtr.Zero;

            try
            {
                // Open the access token of the current process with TOKEN_QUERY.
                if (!NativeMethod.OpenProcessToken(Process.GetCurrentProcess().Handle,
                    NativeMethod.TOKEN_QUERY, out hToken))
                {
                    throw new Win32Exception();
                }

                // Then we must query the size of the integrity level information 
                // associated with the token. Note that we expect GetTokenInformation 
                // to return false with the ERROR_INSUFFICIENT_BUFFER error code 
                // because we've given it a null buffer. On exit cbTokenIL will tell 
                // the size of the group information.
                if (!NativeMethod.GetTokenInformation(hToken,
                    TOKEN_INFORMATION_CLASS.TokenIntegrityLevel, IntPtr.Zero, 0,
                    out cbTokenIL))
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != NativeMethod.ERROR_INSUFFICIENT_BUFFER)
                    {
                        // When the process is run on operating systems prior to 
                        // Windows Vista, GetTokenInformation returns false with the 
                        // ERROR_INVALID_PARAMETER error code because 
                        // TokenIntegrityLevel is not supported on those OS's.
                        throw new Win32Exception(error);
                    }
                }

                // Now we allocate a buffer for the integrity level information.
                pTokenIL = Marshal.AllocHGlobal(cbTokenIL);
                if (pTokenIL == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                // Now we ask for the integrity level information again. This may fail 
                // if an administrator has added this account to an additional group 
                // between our first call to GetTokenInformation and this one.
                if (!NativeMethod.GetTokenInformation(hToken,
                    TOKEN_INFORMATION_CLASS.TokenIntegrityLevel, pTokenIL, cbTokenIL,
                    out cbTokenIL))
                {
                    throw new Win32Exception();
                }

                // Marshal the TOKEN_MANDATORY_LABEL struct from native to .NET object.
                TOKEN_MANDATORY_LABEL tokenIL = (TOKEN_MANDATORY_LABEL)
                    Marshal.PtrToStructure(pTokenIL, typeof(TOKEN_MANDATORY_LABEL));

                // Integrity Level SIDs are in the form of S-1-16-0xXXXX. (e.g. 
                // S-1-16-0x1000 stands for low integrity level SID). There is one 
                // and only one subauthority.
                IntPtr pIL = NativeMethod.GetSidSubAuthority(tokenIL.Label.Sid, 0);
                IL = Marshal.ReadInt32(pIL);
            }
            finally
            {
                // Centralized cleanup for all allocated resources. 
                if (hToken != null)
                {
                    hToken.Close();
                    hToken = null;
                }
                if (pTokenIL != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pTokenIL);
                    pTokenIL = IntPtr.Zero;
                    cbTokenIL = 0;
                }
            }

            return IL;
        }
        
        internal static string GetProcessIntegrityLevelDescription()
        {
            string result;

            switch (GetProcessIntegrityLevel())
            {
                case NativeMethod.SECURITY_MANDATORY_LOW_RID:
                    result = "LOW";
                    break;
                case NativeMethod.SECURITY_MANDATORY_MEDIUM_RID:
                    result = "MEDIUM";
                    break;
                case NativeMethod.SECURITY_MANDATORY_HIGH_RID:
                    result = "HIGH";
                    break;
                default:
                    result = "Unknown";
                    break;
            }

            return result;
        }
        #endregion
    }
}
