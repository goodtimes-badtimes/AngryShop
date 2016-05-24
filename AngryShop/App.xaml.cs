using System;
using System.Diagnostics;
using System.Windows;
using AngryShop.Windows;
using Application = System.Windows.Application;

namespace AngryShop
{
    public partial class App
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;

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

            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.MouseClick += (s, args) => showMainWindow();
            _notifyIcon.Icon = AngryShop.Properties.Resources.find;
            _notifyIcon.Visible = true;

            var menuItemConfig = new System.Windows.Forms.MenuItem("Configuration...");
            menuItemConfig.Click += menuItemConfigurationOnClick;
            var menuItemExit = new System.Windows.Forms.MenuItem("Exit...");
            menuItemExit.Click += (s, args) => exitApplication();
            _notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new[] { menuItemConfig, new System.Windows.Forms.MenuItem("-"), menuItemExit });

            MainWindow.Show();
        }

        private void exitApplication()
        {
            _isExit = true;
            MainWindow.Close();
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        private void showMainWindow()
        {
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                MainWindow.Activate();
            }
            else
            {
                MainWindow.Show();
            }
        }


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
            win.OnCloseWindowSettings += DataManager.OpenConfiguration;
            win.Show();
        }
    }
}
