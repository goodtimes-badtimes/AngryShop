using System;
using System.Windows;
using AngryShop.Items;
using Microsoft.Win32;

namespace AngryShop.Helpers
{
    static class RegistryHelper
    {
        public static void SetLaunchAtStartUp(bool launchAtStartUp)
        {
            try
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                if (rk != null)
                {
                    if (launchAtStartUp)
                        rk.SetValue(Constants.ProgramName, System.Reflection.Assembly.GetExecutingAssembly().Location);
                    else
                        rk.DeleteValue(Constants.ProgramName, false);
                }
            }
            catch (Exception e)
            {
                LogHelper.SaveError(e);
                MessageBox.Show(e.Message);
            }
        }

    }
}
