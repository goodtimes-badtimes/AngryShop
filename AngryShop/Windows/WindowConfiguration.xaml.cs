using System.Windows;
using AngryShop.Entities;
using AngryShop.Helpers;
using AngryShop.Items;

namespace AngryShop.Windows
{
    public partial class WindowConfiguration
    {
        public DialogClosedDelegate OnCloseWindowSettings;
        public WindowConfiguration()
        {
            InitializeComponent();

            DataContext = DataManager.Configuration;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            DataManager.SaveConfiguration();
            RegistryHelper.SetLaunchAtStartUp(((Configuration) DataContext).ToLaunchOnSystemStart);

            DataContext = null;
            Close();
            if (OnCloseWindowSettings != null) OnCloseWindowSettings(true);
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DataContext = null;
            Close();
            if (OnCloseWindowSettings != null) OnCloseWindowSettings(false);
        }

        private void ButtonSetToDefaults_Click(object sender, RoutedEventArgs e)
        {
            ((Configuration)DataContext).SetToDefaultCommonValues();
        }

        private void HyperlinkEditWordsList_OnClick(object sender, RoutedEventArgs e)
        {
            var win = new WindowEditCommonWords {Owner = this};
            win.ShowDialog();
        }

        private void HyperlinkAbout_OnClick(object sender, RoutedEventArgs e)
        {
            var win = new WindowAboutDialog { Owner = this };
            win.ShowDialog();
        }
    }
}
