using Sa11ytaire4All.Views;
using System.Diagnostics;

namespace Sa11ytaire4All
{
    public partial class AppShell : Shell
    {
        private string Sa11ytaireHelpPage =
            "https://www.facebook.com/permalink.php?story_fbid=pfbid036UmuvcKvmqBjkk2m2DRFNx7F7EpNwBJbPgTunUrvPnWebpkHuJikxpibtYijoP6tl&id=61564996280215";

        public AppShell()
        {
            InitializeComponent();
        }

        private async void HelpMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            try
            {
                // Open the Sa11ytaireHelp page on Facebook.
                Uri uri = new Uri(Sa11ytaireHelpPage);

                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Unable to open browser: " + ex.Message);

                // An unexpected error occurred. Perhaps no browser is available on the device.
            }

            //var helpPage = new HelpPage();
            //await Navigation.PushModalAsync(helpPage);
        }

        private async void SettingsMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            var settingsPage = new SettingsPage();
            await Navigation.PushModalAsync(settingsPage);
        }

        private async void RestartGameMenuItem_Clicked(object sender, EventArgs e)
        {
            var answer = await DisplayAlert(
                MainPage.MyGetString("Sa11ytaire"),
                MainPage.MyGetString("QueryRestartGame"),
                MainPage.MyGetString("Yes"),
                MainPage.MyGetString("No"));

            if (answer)
            {
                Shell.Current.FlyoutIsPresented = false;
                
                MainPage.MainPageSingleton?.RestartGame(true);
            }
        }
    }
}
