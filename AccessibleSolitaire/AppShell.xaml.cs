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

        private void RestartGameMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;

            MainPage.MainPageSingleton?.QueryRestartGame();
        }

        private void CloseMenuItem_Clicked(object sender, EventArgs e)
        {
            Shell.Current.FlyoutIsPresented = false;
        }
    }
}
