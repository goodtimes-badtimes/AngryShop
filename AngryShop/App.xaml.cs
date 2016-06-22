using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using AngryShop.Helpers;
using AngryShop.Helpers.Extensions;
using AngryShop.Items;
using AngryShop.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace AngryShop
{
    public partial class App
    {
        private KeyboardHook _hook;
        private System.Windows.Forms.NotifyIcon _notifyIcon;

        private bool _listWindowIsShown;
        private bool _isExit;

        static Mutex _mutex = new Mutex(true, "{8F6F5AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!_mutex.WaitOne(TimeSpan.Zero, true))
            {
                var pr = Process.GetProcessesByName(Constants.ProgramName);
                if (pr.Length > 1)
                {
                    foreach (Process process in pr)
                    {
                        foreach (ProcessThread threadInfo in process.Threads)
                        {
                            IntPtr[] windows = GetWindowHandlesForThread(threadInfo.Id);
                            if (windows != null && windows.Length > 0)
                            {
                                foreach (IntPtr hWnd in windows)
                                {
                                    WinApiHelper.SendMessageToShowMainWindow(hWnd);
                                }
                            }
                        }
                    }
                }
                else // if somebody renamed our .exe file
                {
                    MessageBox.Show("Another instance of the application is already running.", Constants.ProgramName, MessageBoxButton.OK, MessageBoxImage.Information);
                }

                Current.Shutdown();
                return;
            }

            base.OnStartup(e);

            DataManager.ThisProcessId = Process.GetCurrentProcess().Id;
            DataManager.OpenConfiguration();
            DataManager.OpenCommonWords();

            MainWindow = new MainWindow();
            MainWindow.Closing += (sender, args) =>
            {
                if (!_isExit)
                {
                    args.Cancel = true;
                    MainWindow.Hide();
                    _listWindowIsShown = false;
                }
                else
                {
                    DataManager.Configuration.WinPositionX = MainWindow.Left;
                    DataManager.Configuration.WinPositionY = MainWindow.Top;
                    DataManager.Configuration.WinSizeHeight = MainWindow.Height;
                    DataManager.Configuration.WinSizeWidth = MainWindow.Width;
                    DataManager.SaveConfiguration();
                }
            };
            
            MainWindow.Closed += (sender, args) =>
            {
                foreach (Window win in Current.Windows)
                {
                    win.Close();
                }
            };

            ((MainWindow) MainWindow).OnWindowShowing += () =>
            {
                _listWindowIsShown = true;
            };

            _notifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = AngryShop.Properties.Resources.STE_White_MultiImage,
                Visible = true,
                Text = Constants.ProgramName
            };
            _notifyIcon.MouseClick += (s, args) =>
            {
                if (args.Button == MouseButtons.Left)
                {
                    ((MainWindow) MainWindow).ShowWindow();
                }
            };

            var menuItemConfig = new System.Windows.Forms.MenuItem("Configuration...") {DefaultItem = true};
            menuItemConfig.Click += menuItemConfigurationOnClick;
            var menuItemExit = new System.Windows.Forms.MenuItem("Exit Application");
            menuItemExit.Click += (s, args) => exitApplication();
            _notifyIcon.ContextMenu =
                new System.Windows.Forms.ContextMenu(new[]
                {menuItemConfig, new System.Windows.Forms.MenuItem("-"), menuItemExit});

            setWindowVisibilityBehaviour();
            ((MainWindow)MainWindow).ShowWindow();

            _mutex.ReleaseMutex();
        }

        
        /// <summary> Sets app behaviour on tray icon click and hotkey  </summary>
        private void setWindowVisibilityBehaviour(bool toShowBalloonTip = false)
        {
            if (DataManager.Configuration.ToDisplayListOnHotkey)
            {
                if (_hook == null)
                {
                    try
                    {
                        _hook = new KeyboardHook();
                        // Register the event that is fired after the key press
                        _hook.KeyPressed += hook_KeyPressed;
                        // Register the Ctrl+Alt+S combination as hotkey (Alt Gr = Ctrl+Alt)
                        _hook.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt, Keys.S);
                    }
                    catch (InvalidOperationException exception)
                    {
                        MessageBox.Show(exception.Message);
                        LogHelper.SaveError(exception);
                    }
                }
            }
            else
            {
                if (_hook != null)
                {
                    _hook.Dispose();
                    _hook = null;
                }
            }

            if (toShowBalloonTip)
            {
                if (!DataManager.Configuration.ToDisplayListOnTextFocus)
                {
                    _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    _notifyIcon.BalloonTipTitle = Constants.ProgramName;
                    _notifyIcon.BalloonTipText = @"Word List will appear on tray icon click";
                    if (DataManager.Configuration.ToDisplayListOnHotkey)
                        _notifyIcon.BalloonTipText += @" or ""Ctrl+Alt+S"" hotkey";
                    _notifyIcon.ShowBalloonTip(3000);
                }
            }
        }

        private void exitApplication()
        {
            _isExit = true;
            MainWindow.Close();
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        /// <summary> "Configuration..." menu item </summary>
        private void menuItemConfigurationOnClick(object sender, EventArgs eventArgs)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is WindowConfiguration)
                {
                    window.Activate();
                    return;
                }
            }

            var win = new WindowConfiguration();
            win.OnCloseWindowSettings += saved =>
            {
                DataManager.OpenConfiguration();
                setWindowVisibilityBehaviour(saved && !_listWindowIsShown);
            };
            win.Show();
        }

        /// <summary> Global hotkey pressed event handler  </summary>
        void hook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (!_listWindowIsShown)
                ((MainWindow)MainWindow).ShowWindow();
            else MainWindow.Close();
        }








        #region [   ]

        private static IntPtr[] GetWindowHandlesForThread(int threadHandle)
        {
            _results.Clear();
            EnumWindows(WindowEnum, threadHandle);
            return _results.ToArray();
        }

        // enum windows

        private delegate int EnumWindowsProc(IntPtr hwnd, int lParam);

        [DllImport("user32.Dll")]
        private static extern int EnumWindows(EnumWindowsProc x, int y);
        [DllImport("user32")]
        private static extern bool EnumChildWindows(IntPtr window, EnumWindowsProc callback, int lParam);
        [DllImport("user32.dll")]
        public static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        private static List<IntPtr> _results = new List<IntPtr>();

        private static int WindowEnum(IntPtr hWnd, int lParam)
        {
            int processID = 0;
            int threadID = GetWindowThreadProcessId(hWnd, out processID);
            if (threadID == lParam)
            {
                _results.Add(hWnd);
                EnumChildWindows(hWnd, WindowEnum, threadID);
            }
            return 1;
        }

        // get window text

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        private static string GetText(IntPtr hWnd)
        {
            int length = GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        // get richedit text 

        public const int GWL_ID = -12;
        public const int WM_GETTEXT = 0x000D;

        [DllImport("User32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int index);
        [DllImport("User32.dll")]
        public static extern IntPtr SendDlgItemMessage(IntPtr hWnd, int IDDlgItem, int uMsg, int nMaxCount, StringBuilder lpString);
        [DllImport("User32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        private static StringBuilder GetEditText(IntPtr hWnd)
        {
            Int32 dwID = GetWindowLong(hWnd, GWL_ID);
            IntPtr hWndParent = GetParent(hWnd);
            StringBuilder title = new StringBuilder(128);
            SendDlgItemMessage(hWndParent, dwID, WM_GETTEXT, 128, title);
            return title;
        }

        #endregion
    }
}
