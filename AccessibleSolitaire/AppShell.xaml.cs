using Sa11ytaire4All.Views;
using System.Diagnostics;

namespace Sa11ytaire4All
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private async void HelpMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            MainPage.MainPageSingleton?.LaunchHelp();
        }

        private async void SettingsMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            var settingsPage = new SettingsPage();
            await Navigation.PushModalAsync(settingsPage);
        }

        private async void RestartGameMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            MainPage.MainPageSingleton?.QueryRestartGame();
        }
    }
}
