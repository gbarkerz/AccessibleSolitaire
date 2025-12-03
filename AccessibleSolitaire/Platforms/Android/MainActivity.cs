using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.View;

namespace Sa11ytaire4All
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
        ResizeableActivity = true, LaunchMode = LaunchMode.SingleTask)]
    [Register("a11y.gbarker.accessiblesolitaire.MainActivity")]
    public class MainActivity : MauiAppCompatActivity
    {
        // On Android 26, the app window is sized to fill the entire screen, including behind the 
        // status bar at the top of the screen and the navigation bar at the bottom. So add some
        // padding to those bars to push the app window to lie between the two.
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Window != null)
            {
                ViewCompat.SetOnApplyWindowInsetsListener(Window.DecorView, new InsetsListener());
            }                    
        }

        public class InsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
        {
            public WindowInsetsCompat? OnApplyWindowInsets(Android.Views.View? v, WindowInsetsCompat? insets)
            {
                if ((v == null) || (insets == null))
                {
                    return insets;
                }

                var systemBars = insets.GetInsets(WindowInsetsCompat.Type.SystemBars() | WindowInsetsCompat.Type.DisplayCutout());
                if (systemBars != null)
                {
                    v.SetPadding(systemBars.Left, systemBars.Top, systemBars.Right, systemBars.Bottom);
                }

                return insets;
            }
        }
    }
}
