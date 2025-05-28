using System.Diagnostics;

namespace Sa11ytaire4All
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            // Apply minimum dimensions for the main app window.
#if WINDOWS
            window.MinimumWidth = 600;
            window.MinimumHeight = 400;
#endif

            // On Windows, we could use Destroying() here, but that's no guaranteed on mobile apparently,
            // (and didn't work when tested on Android). So use Deactivating instead. The work being done
            // here should be completed pretty quick.
            window.Deactivated += (s, e) =>
            {
                Debug.WriteLine("Soliatire: App window deactivating.");

                var singleton = Sa11ytaire4All.MainPage.MainPageSingleton;
                if (singleton != null)
                {
                    singleton.SaveSession();
                }
            };

            return window;
        }
    }
}
