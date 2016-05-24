using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AngryShop
{
    static class WinApiHelper
    {
        [DllImport("user32.dll")]
        static extern int GetFocus();

        [DllImport("user32.dll")]
        static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(int hWnd, int ProcessId);

        [DllImport("user32.dll")]
        static extern int GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        static extern int SendMessage(int hWnd, int Msg, int wParam, StringBuilder lParam);

        const int WM_SETTEXT = 12;
        const int WM_GETTEXT = 13;


        public static string GetActiveWindowText()
        {
            var builder = new StringBuilder(50000);

            int foregroundWindowHandle = GetForegroundWindow();
            uint remoteThreadId = GetWindowThreadProcessId(foregroundWindowHandle, 0);
            uint currentThreadId = GetCurrentThreadId();

            //AttachTrheadInput is needed so we can get the handle of a focused window in another app
            AttachThreadInput(remoteThreadId, currentThreadId, true);
            //Get the handle of a focused window
            int focused = GetFocus();
            //Now detach since we got the focused handle
            AttachThreadInput(remoteThreadId, currentThreadId, false);

            //Get the text from the active window into the stringbuilder
            SendMessage(focused, WM_GETTEXT, builder.Capacity, builder);
            //Console.WriteLine("Text in active window was " + builder);
            //builder.Append(" Extra text");
            //Change the text in the active window
            //SendMessage(focused, WM_SETTEXT, 0, builder);

            return builder.ToString();
        }
    }
}
