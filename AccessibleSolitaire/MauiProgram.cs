using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Plugin.Maui.KeyListener;
using Sa11ytaire4All.Source;
using Sa11ytaire4All.Views;
using System.Diagnostics;

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

                    events.AddWindows(windows => windows.OnPlatformMessage((window, args) =>
                    {
                        if (!addedKeyEventHandler && window.Content != null)
                        {
                            addedKeyEventHandler = true;

                            window.Content.PreviewKeyDown += MyPreviewKeyDownEventHandler;
                        }
                    }));
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

        private static bool addedKeyEventHandler = false;

        static void MyPreviewKeyDownEventHandler(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.Key == Windows.System.VirtualKey.Z)
            {
                // Note: Context menus seem to appear in response to a right click, but not in response
                // to a press of the Context Menu key (VirtualKey.Application). So add a shortcut to 
                // show the zoom card popup.

                // Is keyboard focus on a CardButton?
                var focusedCardButtonCard = CardButton.FocusedCardButtonCard;
                if (focusedCardButtonCard != null)
                {
                    MainPage.MainPageSingleton?.ShowZoomedCardPopup(focusedCardButtonCard, false);
                }
                else
                {
                    var dealtCard = GetDealtCardFromListViewItem(e);
                    if (dealtCard != null)
                    {
                        MainPage.MainPageSingleton?.ShowZoomedCardPopup(dealtCard.Card, true);
                    }
                }
            }
            else if ((e.Key == Windows.System.VirtualKey.Space) ||
                (e.Key == Windows.System.VirtualKey.Enter))
            {
                e.Handled = false;

                var dealtCard = GetDealtCardFromListViewItem(e);
                if (dealtCard != null)
                {
                    // Always allow the default action when Space or Enter is pressed at a dealt card.
                    // After that, perform the appropriate selection or deselection ourselves.
                    if (dealtCard == MostRecentDealtCardKeyboardSpaceOrEnter)
                    {
                        MainPage.MainPageSingleton?.ToggleDealtCardSelectionFollowingKeyPress(dealtCard);

                        e.Handled = true;
                    }

                    MostRecentDealtCardKeyboardSpaceOrEnter = dealtCard;
                }

                Debug.WriteLine("Process Space or Enter press: Handled " + e.Handled);
            }
            else
            {
                e.Handled = false;
            }
        }

#if WINDOWS
        static private DealtCard? MostRecentDealtCardKeyboardSpaceOrEnter = null;

        static private DealtCard? GetDealtCardFromListViewItem(Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            DealtCard? dealtCard = null;

            var listViewItem = e.OriginalSource as Microsoft.UI.Xaml.Controls.ListViewItem;
            if ((listViewItem != null) && (listViewItem.Content != null))
            {
                var contentTemplateRoot = ((Microsoft.Maui.Controls.Platform.ItemContentControl)listViewItem.ContentTemplateRoot);
                if ((contentTemplateRoot != null) && (contentTemplateRoot.FormsDataContext != null))
                {
                    dealtCard = contentTemplateRoot.FormsDataContext as DealtCard;
                }
            }

            return dealtCard;
        }
#endif


#endif


    }
}

