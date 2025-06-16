using Android.App;
using Android.Content.PM;
using Android.Runtime;

namespace Sa11ytaire4All
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
        ResizeableActivity = true, LaunchMode = LaunchMode.SingleTask)]
    [Register("a11y.gbarker.accessiblesolitaire.MainActivity")]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}
