using System;
using System.Diagnostics;
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
                MessageBox.Show("Another instance of the application is already running.", Constants.ProgramName, MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}
