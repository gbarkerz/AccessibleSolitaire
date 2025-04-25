using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Input;
using Windows.System;
using Windows.UI.Core;

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
                    // Barker Todo: Investigate the advantages of CollectionViewHandler2.
                    // handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();

#if WINDOWS
                    Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("KeyboardAccessibleCollectionView", (handler, view) =>
                    {
                        handler.PlatformView.SingleSelectionFollowsFocus = false;
                });
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSansRegular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSansSemibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("fontawesome6freesolid900.otf", "FA");
                });

#if WINDOWS
            builder
                .UseMauiApp<App>()
                .ConfigureLifecycleEvents(events =>
                  {
                      events.AddWindows(windows => windows
                        .OnWindowCreated(window =>
                        {
                            window.SizeChanged += OnSizeChanged;
                        }));
                  });
#endif

#if WINDOWS
            builder
                .UseMauiApp<App>()
                .ConfigureLifecycleEvents(events =>
                {
                    events.AddWindows(windows => windows.OnPlatformMessage((window, args) =>
                    {
                        if (!addedKeyEventHandler && window.Content != null)
                        {
                            addedKeyEventHandler = true;

                            window.Content.PreviewKeyDown += MyPreviewKeyDownEventHandler;
                        }
                    }));
                });
#endif

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

#if WINDOWS
        private static bool addedKeyEventHandler = false;

        static void MyPreviewKeyDownEventHandler(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.Key == Windows.System.VirtualKey.F1)
            {
                MainPage.MainPageSingleton?.LaunchHelp();
            }
            else if (e.Key == Windows.System.VirtualKey.F2)
            {
                MainPage.MainPageSingleton?.AnnounceStateRemainingCards();
            }
            else if (e.Key == Windows.System.VirtualKey.F3)
            {
                MainPage.MainPageSingleton?.AnnounceStateTargetPiles();
            }
            else if (e.Key == Windows.System.VirtualKey.F4)
            {
                MainPage.MainPageSingleton?.AnnounceStateDealtCardPiles();
            }
            else if (e.Key == Windows.System.VirtualKey.F5)
            {
                MainPage.MainPageSingleton?.QueryRestartGame();
            }
            else if (e.Key == Windows.System.VirtualKey.F6)
            {
                var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
                var shiftDown = shiftState.HasFlag(CoreVirtualKeyStates.Down);

                MainPage.MainPageSingleton?.HandleF6(!shiftDown);
            }
            else if (e.Key == Windows.System.VirtualKey.F7)
            {
                MainPage.MainPageSingleton?.AnnounceAvailableMoves();
            }
            else
            {
                e.Handled = false;
            }
        }
#endif
    }
}

