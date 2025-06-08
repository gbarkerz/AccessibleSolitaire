using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Maui.KeyListener;


#if WINDOWS
using Microsoft.UI.Input;
using Windows.System;
using Windows.UI.Core;
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
                .UseKeyListener()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureMauiHandlers(handlers =>
                {
                    // Barker Todo: Investigate the advantages of CollectionViewHandler2.
                    // handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();

#if WINDOWS
                    Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
                    {
                        handler.PlatformView.SingleSelectionFollowsFocus = false;
                    });
#endif
                })
                .ConfigureLifecycleEvents(events =>
                {
#if WINDOWS
                    events.AddWindows(windows => windows
                        .OnWindowCreated(window =>
                        {
                            window.SizeChanged += OnSizeChanged;
                        }
                        ));
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSansRegular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSansSemibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("fontawesome6freesolid900.otf", "FA");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }


#if WINDOWS
    static void OnSizeChanged(object sender, Microsoft.UI.Xaml.WindowSizeChangedEventArgs args)
    {
        var singleton = MainPage.MainPageSingleton;
        if (singleton != null)
        {
            singleton.OriginalCardWidth = 0;
            singleton.OriginalCardHeight = 0;
        }
    }
#endif


    }
}

