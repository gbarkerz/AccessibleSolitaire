
using Microsoft.Maui.Controls;
using Sa11ytaire4All.ViewModels;
using System;
using System.Diagnostics;

namespace Sa11ytaire4All
{
    // This file contains code relating to changing the layout of the app as sceen orientation changes.
    public partial class MainPage : ContentPage
    {
        static private bool? initialScreenOrientationPortrait = null;

        private bool processingScreenOrientationChange = false;

        public void SetOrientationLayout()
        {
            if (processingScreenOrientationChange)
            {
                return;
            }

            processingScreenOrientationChange = true;

            var isPortrait = MainPage.IsPortrait();

            Debug.WriteLine("SetOrientationLayout: initialScreenOrientationPortrait " + initialScreenOrientationPortrait);

            SetUpperGridViewOrientationLayout(isPortrait);

            var changedLayout = true;
#if IOS

            // Have we already set the UI layout to account for the screen orientation?
            if (initialScreenOrientationPortrait != null)
            {
                // Unfortunately, at the time of writing this, CollectionViews on iOS don't support
                // having their ItemsLayout changed. As such we cannot support a change in screen
                // orientation while the app's running.
                if ((bool)initialScreenOrientationPortrait != isPortrait)
                {
                    InnerMainGrid.IsVisible = false;
                    CardPileGrid.IsVisible = false;

                    Debug.WriteLine("SetOrientationLayout: NoScreenOrientationChangeLabel.IsVisible " + NoScreenOrientationChangeLabel.IsVisible);

                    if (!NoScreenOrientationChangeLabel.IsVisible)
                    {
                        var portraitString = MainPage.MyGetString("Portrait");
                        var landcapeString = MainPage.MyGetString("Landscape");
                        var noScreenOrientationChangeString = MainPage.MyGetString("NoScreenOrientationChange");

                        var message = string.Format(noScreenOrientationChangeString,
                                                        (bool)initialScreenOrientationPortrait ?
                                                            portraitString : landcapeString);

                        var vm = this.BindingContext as DealtCardViewModel;
                        if (vm != null)
                        {
                            vm.NoScreenOrientationChangeWarning = message;
                        }

                        NoScreenOrientationChangeLabel.IsVisible = true;

                        // Set semantic focus to the newly shown label in order to have it announced by a screen reader.
                        //NoScreenOrientationChangeLabel.SetSemanticFocus();

                        // Note: The above attempt to have the label announced doesn't seem to work because focus
                        // gets pulled back to the Menu button. So instead, just manually announce the label text.
                        if (timerDelayScreenReaderAnnouncement == null)
                        {
                            timerDelayScreenReaderAnnouncement = new Timer(
                                new TimerCallback((s) => MakeScreenReaderAnnouncement(message, true)),
                                                            message,
                                                            TimeSpan.FromMilliseconds(2000),
                                                            TimeSpan.FromMilliseconds(Timeout.Infinite));
                        }
                    }

                    changedLayout = false;
                }
            }
            else
            {
                // Only set this once.
                initialScreenOrientationPortrait = isPortrait;
            }
#endif

            SetLowerScrollViewOrientationLayout(isPortrait);

            var isKlondike = (currentGameType != SolitaireGameType.Pyramid);
            //Debug.WriteLine("SetOrientationLayout: isKlondike " + isKlondike);

            if (changedLayout)
            {
                InnerMainGrid.IsVisible = true;

                CardPileGrid.IsVisible = isKlondike;

                NoScreenOrientationChangeLabel.IsVisible = false;
            }

            processingScreenOrientationChange = false;
        }

        private void SetUpperGridViewOrientationLayout(bool isPortrait)
        {
            if (isPortrait)
            {
                Grid.SetRowSpan(UpperGrid, 2);

                Grid.SetColumnSpan(TopCornerPiles, 2);

                Grid.SetRow(TargetPiles, 1);
                Grid.SetColumn(TargetPiles, 1);
                Grid.SetColumnSpan(TargetPiles, 2);
            }
            else
            {
                Grid.SetRowSpan(UpperGrid, 3);

                Grid.SetColumnSpan(TopCornerPiles, 1);

                Grid.SetRow(TargetPiles, 0);
                Grid.SetColumn(TargetPiles, 2);
                Grid.SetColumnSpan(TargetPiles, 1);
            }
        }

        private void SetLowerScrollViewOrientationLayout(bool isPortrait)
        {
            int cardPileGridRow;
            int cardPileGridRowSpan;

            if (isPortrait)
            {
                cardPileGridRow = 2;
                cardPileGridRowSpan = 7;
            }
            else
            {
                cardPileGridRow = 3;
                cardPileGridRowSpan = 6;
            }

            Grid.SetRow(CardPileGrid, cardPileGridRow);
            Grid.SetRowSpan(CardPileGrid, cardPileGridRowSpan);

            Grid.SetRow(CardPileGridPyramid, cardPileGridRow);
            Grid.SetRowSpan(CardPileGridPyramid, cardPileGridRowSpan);

            SetCollectionViewsOrientationLayout(isPortrait);
        }

        private void SetCollectionViewsOrientationLayout(bool isPortrait)
        {
            SetCollectionViewOrientationLayout(isPortrait, 0, CardPile1);
            SetCollectionViewOrientationLayout(isPortrait, 1, CardPile2);
            SetCollectionViewOrientationLayout(isPortrait, 2, CardPile3);
            SetCollectionViewOrientationLayout(isPortrait, 3, CardPile4);
            SetCollectionViewOrientationLayout(isPortrait, 4, CardPile5);
            SetCollectionViewOrientationLayout(isPortrait, 5, CardPile6);
            SetCollectionViewOrientationLayout(isPortrait, 6, CardPile7);
        }

        private void SetCollectionViewOrientationLayout(bool isPortrait, int index, CollectionView collectionView)
        {
            if (isPortrait)
            {
                Grid.SetRow(collectionView, index);
                Grid.SetRowSpan(collectionView, 1);

                Grid.SetColumn(collectionView, 0);
                Grid.SetColumnSpan(collectionView, 7);

                collectionView.ItemsLayout = LinearItemsLayout.Horizontal;
            }
            else
            {
                Grid.SetRow(collectionView, 0);
                Grid.SetRowSpan(collectionView, 7);

                Grid.SetColumn(collectionView, index);
                Grid.SetColumnSpan(collectionView, 1);

                collectionView.ItemsLayout = LinearItemsLayout.Vertical;
            }
        }
    }
}
