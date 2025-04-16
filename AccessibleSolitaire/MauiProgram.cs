using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

#if WINDOWS
//using Microsoft.Maui.LifecycleEvents;
#endif 

namespace Sa11ytaire4All
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureMauiHandlers(handlers =>
                {
#if IOS
                    // Barker Todo: Investigate the advantages of CollectionViewHandler2.
                    // handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
#endif
                    Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
                    {
#if WINDOWS
                        handler.PlatformView.SingleSelectionFollowsFocus = false;
#endif
                    });
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSansRegular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSansSemibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("fontawesome6freesolid900.otf", "FA");
                });

#if WINDOWS
        // Barker: If we ever need to add a KeyDown hook.
        //builder
        //    .UseMauiApp<App>()
        //    .ConfigureLifecycleEvents(events =>
        //    {
        //        events.AddWindows(windows => windows.OnPlatformMessage((window, args) =>
        //        {
        //            if (!addedKeyEventHandler && window.Content != null)
        //            {
        //                addedKeyEventHandler = true;

        //                window.Content.PreviewKeyDown += MyPreviewKeyDownEventHandler;
        //            }
        //        }));
        //    });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

#if WINDOWS
        //private static bool addedKeyEventHandler = false;

        //static void MyPreviewKeyDownEventHandler(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        //{
        //    if ((e.Key == Windows.System.VirtualKey.Space) ||
        //        (e.Key == Windows.System.VirtualKey.Enter))
        //    {
        //        Do work here...
        //
        //        e.Handled = true;
        //    }
        //}
#endif
    }
}

