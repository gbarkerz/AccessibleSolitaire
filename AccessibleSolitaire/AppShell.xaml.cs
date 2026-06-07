using Sa11ytaire4All.Services;
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

        private void KlondikeSolitaireMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            if (MainPage.currentGameType != SolitaireGameType.Klondike)
            {
                MainPage.MainPageSingleton?.LoadKlondikeGame();
            }
        }

        private void BakersdozenSolitaireMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            if (MainPage.currentGameType != SolitaireGameType.Bakersdozen)
            {
                MainPage.MainPageSingleton?.LoadBakersdozenGame();
            }
        }

        private void GrandfathersclockSolitaireMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            if (MainPage.currentGameType != SolitaireGameType.Grandfathersclock)
            {
                MainPage.MainPageSingleton?.LoadGrandfathersclockGame();
            }
        }

        private void PyramidSolitaireMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            if (MainPage.currentGameType != SolitaireGameType.Pyramid)
            {
                MainPage.MainPageSingleton?.LoadPyramidGame();
            }
        }

        private void TripeaksSolitaireMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            if (MainPage.currentGameType != SolitaireGameType.Tripeaks)
            {
                MainPage.MainPageSingleton?.LoadTripeaksGame();
            }
        }

        private void SpiderSolitaireMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            if (MainPage.currentGameType != SolitaireGameType.Spider)
            {
                MainPage.MainPageSingleton?.LoadSpiderGame();
            }
        }

        private void RoyalparadeSolitaireMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            if (MainPage.currentGameType != SolitaireGameType.Royalparade)
            {
                MainPage.MainPageSingleton?.LoadRoyalparadeGame();
            }
        }

        private void HelpMenuItem_Clicked(object sender, EventArgs e)
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

        private async void YourDeviceMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            string? model = DeviceInfoService.Model();
            if (model == null)
            {
                model = "Unknown";
            }


            string? platform = DeviceInfoService.Platform();
            if (platform == null)
            {
                platform = "Unknown";
            }

            var yourDeviceMessage = MainPage.MyGetString("YourDeviceDetails") + "\n\n" +
                MainPage.MyGetString("Model") + " " + model + "\n\n" +
                MainPage.MyGetString("Platform") + " " + platform;

            await DisplayAlertAsync(
                MainPage.MyGetString("AccessibleSolitaire"),
                yourDeviceMessage,
                MainPage.MyGetString("OK"));
        }

        private void RestartThisGameMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            MainPage.MainPageSingleton?.QueryRestartGame(false);
        }

        private void StartNewGameMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            MainPage.MainPageSingleton?.QueryRestartGame(true);
        }

        private void UndoMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            MainPage.MainPageSingleton?.UndoLastMove();
        }

        private void CloseMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;
        }
    }
}
