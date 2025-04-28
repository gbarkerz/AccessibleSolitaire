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

            return window;
        }
    }
}
