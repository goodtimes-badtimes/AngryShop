using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AngryShop.Helpers
{
    public static class WinApiHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(int hWnd, int Msg, int wparam, int lparam);


        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern int RegisterWindowMessage(string msgName, int id);


        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;


        public const int WM_GETTEXT = 0x000D;
        public const int WM_GETTEXTLENGTH = 0x000E;


        public static readonly int WM_SHOWFIRSTINSTANCE = RegisterWindowMessage("WM_SHOWFIRSTINSTANCE|{0}", 250);

        /// <summary> Sends a message to another instance of itself with code to show main window </summary>
        /// <param name="hwnd">Window descriptor</param>
        public static bool SendMessageToShowMainWindow(IntPtr hWnd)
        {
            long result = SendMessage(hWnd, WM_SHOWFIRSTINSTANCE, IntPtr.Zero, IntPtr.Zero);

            if (result == 0) return true;
            else return false;
        }

        /// <summary> Disables maximize window functionality </summary>
        /// <param name="hwnd">Window descriptor</param>
        public static void DisableMaximizeFunctionality(IntPtr hwnd)
        {
            var value = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MAXIMIZEBOX));
        }
    }
}
