using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;

namespace AngryShop.Windows
{
    public partial class WindowAboutDialog
    {
        public WindowAboutDialog()
        {
            InitializeComponent();

            TextBlockVersion.Text = string.Format("Version {0}", Assembly.GetExecutingAssembly().GetName().Version);
        }

        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void handleLinkClick(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var hl = (Hyperlink)sender;
            string navigateUri = hl.NavigateUri.ToString();
            Process.Start(new ProcessStartInfo(navigateUri));
            e.Handled = true;
        }
    }
}
