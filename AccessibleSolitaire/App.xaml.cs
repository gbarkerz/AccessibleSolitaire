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

            window.Destroying += (s, e) =>
            {
                Debug.WriteLine("Soliatire: App window destroying.");

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
