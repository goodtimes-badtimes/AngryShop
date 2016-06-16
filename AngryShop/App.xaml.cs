using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using AngryShop.Helpers.Extensions;
using AngryShop.Items;
using AngryShop.Items.Enums;
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

        
        protected override void OnStartup(StartupEventArgs e)
        {
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
                    if (DataManager.Configuration.ListVisibilityType == ListVisibilityTypes.OnTrayIconClick)
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
        }

        /// <summary> Sets app behaviour on tray icon click and hotkey  </summary>
        private void setWindowVisibilityBehaviour(bool toShowBalloonTip = false)
        {
            if (DataManager.Configuration.ListVisibilityType == ListVisibilityTypes.OnHotkey)
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
                }
            }
            else
            {
                _hook = null;
            }

            if (toShowBalloonTip)
            {
                if (DataManager.Configuration.ListVisibilityType == ListVisibilityTypes.OnTrayIconClick ||
                DataManager.Configuration.ListVisibilityType == ListVisibilityTypes.OnHotkey)
                {
                    _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                    _notifyIcon.BalloonTipTitle = Constants.ProgramName;
                    _notifyIcon.BalloonTipText = DataManager.Configuration.ListVisibilityType ==
                        ListVisibilityTypes.OnTrayIconClick
                        ? @"Program is still running. It will appear on tray icon click"
                        : @"Program is still running. It will appear on ""Ctrl+Alt+S"" hotkey";
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


        //private void showMainWindow()
        //{
        //    if (MainWindow.IsVisible)
        //    {
        //        if (MainWindow.WindowState == WindowState.Minimized)
        //        {
        //            MainWindow.WindowState = WindowState.Normal;
        //        }
        //        MainWindow.Activate();
        //    }
        //    else
        //    {
        //        MainWindow.Show();
        //    }
        //    _listWindowIsShown = true;
        //}


        /// <summary> "Configuration..." menu item </summary>
        private void menuItemConfigurationOnClick(object sender, EventArgs eventArgs)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is WindowSettings)
                {
                    window.Activate();
                    return;
                }
            }

            var win = new WindowSettings();
            win.OnCloseWindowSettings += () =>
            {
                DataManager.OpenConfiguration();
                setWindowVisibilityBehaviour(!_listWindowIsShown);
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
