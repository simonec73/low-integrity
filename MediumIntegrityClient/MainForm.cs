﻿using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace MediumIntegrityClient
{
    public partial class MainForm : Form
    {
        private NamedPipeClientStream _namedPipe;

        public MainForm()
        {
            InitializeComponent();

            this.Text = string.Format("Launcher - {0} Integrity Level", GetProcessIntegrityLevelDescription());
        }

        #region Helper Functions related to Process Integrity Level
        /// <summary>
        /// The function launches an application at low integrity level. 
        /// </summary>
        /// <param name="commandLine">
        /// The command line to be executed. The maximum length of this string is 32K 
        /// characters. 
        /// </param>
        /// <remarks>
        /// To start a low-integrity process, 
        /// 1) Duplicate the handle of the current process, which is at medium 
        ///    integrity level.
        /// 2) Use SetTokenInformation to set the integrity level in the access token 
        ///    to Low.
        /// 3) Use CreateProcessAsUser to create a new process using the handle to 
        ///    the low integrity access token.
        /// </remarks>
        internal void CreateLowIntegrityProcess(string commandLine, bool hide = false)
        {
            SafeTokenHandle hToken = null;
            SafeTokenHandle hNewToken = null;
            IntPtr pIntegritySid = IntPtr.Zero;
            int cbTokenInfo = 0;
            IntPtr pTokenInfo = IntPtr.Zero;
            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();

            try
            {
                // Open the primary access token of the process.
                if (!NativeMethod.OpenProcessToken(Process.GetCurrentProcess().Handle,
                    NativeMethod.TOKEN_DUPLICATE | NativeMethod.TOKEN_ADJUST_DEFAULT |
                    NativeMethod.TOKEN_QUERY | NativeMethod.TOKEN_ASSIGN_PRIMARY,
                    out hToken))
                {
                    throw new Win32Exception();
                }

                // Duplicate the primary token of the current process.
                if (!NativeMethod.DuplicateTokenEx(hToken, 0, IntPtr.Zero,
                    SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                    TOKEN_TYPE.TokenPrimary, out hNewToken))
                {
                    throw new Win32Exception();
                }

                // Create the low integrity SID.
                if (!NativeMethod.AllocateAndInitializeSid(
                    ref NativeMethod.SECURITY_MANDATORY_LABEL_AUTHORITY, 1,
                    NativeMethod.SECURITY_MANDATORY_LOW_RID,
                    0, 0, 0, 0, 0, 0, 0, out pIntegritySid))
                {
                    throw new Win32Exception();
                }

                TOKEN_MANDATORY_LABEL tml;
                tml.Label.Attributes = NativeMethod.SE_GROUP_INTEGRITY;
                tml.Label.Sid = pIntegritySid;

                // Marshal the TOKEN_MANDATORY_LABEL struct to the native memory.
                cbTokenInfo = Marshal.SizeOf(tml);
                pTokenInfo = Marshal.AllocHGlobal(cbTokenInfo);
                Marshal.StructureToPtr(tml, pTokenInfo, false);

                // Set the integrity level in the access token to low.
                if (!NativeMethod.SetTokenInformation(hNewToken,
                    TOKEN_INFORMATION_CLASS.TokenIntegrityLevel, pTokenInfo,
                    cbTokenInfo + NativeMethod.GetLengthSid(pIntegritySid)))
                {
                    throw new Win32Exception();
                }

                // Create the new process at the Low integrity level.
                si.cb = Marshal.SizeOf(si);
                uint flags = hide ? 0x08000000u : 0u;
                if (!NativeMethod.CreateProcessAsUser(hNewToken, null, commandLine,
                    IntPtr.Zero, IntPtr.Zero, false, flags, IntPtr.Zero, null, ref si,
                    out pi))
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                // Centralized cleanup for all allocated resources. 
                if (hToken != null)
                {
                    hToken.Close();
                    hToken = null;
                }
                if (hNewToken != null)
                {
                    hNewToken.Close();
                    hNewToken = null;
                }
                if (pIntegritySid != IntPtr.Zero)
                {
                    NativeMethod.FreeSid(pIntegritySid);
                    pIntegritySid = IntPtr.Zero;
                }
                if (pTokenInfo != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pTokenInfo);
                    pTokenInfo = IntPtr.Zero;
                    cbTokenInfo = 0;
                }
                if (pi.hProcess != IntPtr.Zero)
                {
                    NativeMethod.CloseHandle(pi.hProcess);
                    pi.hProcess = IntPtr.Zero;
                }
                if (pi.hThread != IntPtr.Zero)
                {
                    NativeMethod.CloseHandle(pi.hThread);
                    pi.hThread = IntPtr.Zero;
                }
            }
        }


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
        internal int GetProcessIntegrityLevel()
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

        internal string GetProcessIntegrityLevelDescription()
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

        private void _connect_Click(object sender, EventArgs e)
        {
            try
            {
                _namedPipe = new NamedPipeClientStream(".", "LowIntegrityNamedPipe",
                    PipeDirection.InOut,
                    PipeOptions.None);
                _namedPipe.Connect(100);

                _connect.Enabled = false;
                _close.Enabled = true;
                _getAnotherString.Enabled = true;

                byte[] buffer = new byte[100];
                int len = _namedPipe.Read(buffer, 0, 100);
                string text = System.Text.Encoding.UTF8.GetString(buffer, 0, len);
                StringBuilder builder = new StringBuilder(this._text.Text);
                builder.AppendLine(text);
                this._text.Text = builder.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "Failed");
                return;
            }

            MessageBox.Show(this, "Connected.", "Success");

        }

        private void _close_Click(object sender, EventArgs e)
        {
            if (_namedPipe != null)
            {
                try
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes("Close");
                    _namedPipe.Write(bytes, 0, bytes.Length);
                    _namedPipe.Flush();
                }
                catch (Exception exc)
                {
                    _namedPipe = null;
                    _close.Enabled = false;
                    _getAnotherString.Enabled = false;
                    _startServer.Enabled = true;
                    MessageBox.Show(this, exc.Message, "Error");
                }
            }
        }

        private void _getAnotherString_Click(object sender, EventArgs e)
        {
            if (_namedPipe != null)
            {
                try
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes("GetAnotherString");
                    _namedPipe.Write(bytes, 0, bytes.Length);
                    _namedPipe.Flush();

                    byte[] buffer = new byte[100];
                    int len = _namedPipe.Read(buffer, 0, 100);
                    string text = System.Text.Encoding.UTF8.GetString(buffer, 0, len);
                    StringBuilder builder = new StringBuilder(this._text.Text);
                    builder.AppendLine(text);
                    this._text.Text = builder.ToString();
                }
                catch (Exception exc)
                {
                    _namedPipe = null;
                    _close.Enabled = false;
                    _getAnotherString.Enabled = false;
                    _startServer.Enabled = true;
                    MessageBox.Show(this, exc.Message, "Error");
                }
            }
        }

        private void _startServer_Click(object sender, EventArgs e)
        {
            CreateLowIntegrityProcess("LowIntegrityServer.exe", _hiddenServer.Checked);

            _startServer.Enabled = false;
            _connect.Enabled = true;
        }

        private void _clear_Click(object sender, EventArgs e)
        {
            this._text.Text = string.Empty;
        }
    }
}
