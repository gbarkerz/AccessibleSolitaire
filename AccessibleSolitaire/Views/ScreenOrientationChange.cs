
using Sa11ytaire4All.ViewModels;
using System.Diagnostics;

namespace Sa11ytaire4All
{
    // This file contains code relating to changing the layout of the app as sceen orientation changes.
    public partial class MainPage : ContentPage
    {
        static private bool? initialScreenOrientationPortrait = null;

        private bool processingScreenOrientationChange = false;

        // SetOrientationLayout() is called when the app starts, when the display info changes, 
        // when the size of InnerMainPageGrid changes, and when the current game changes.
        public void SetOrientationLayout()
        {
            if (processingScreenOrientationChange)
            {
                return;
            }

            processingScreenOrientationChange = true;

            var isPortrait = (DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait);

            Debug.WriteLine("SetOrientationLayout: initialScreenOrientationPortrait " + initialScreenOrientationPortrait);

            var changedLayout = true;

#if WINDOWS
            Debug.WriteLine("SetOrientationLayout: On Windows, don't show the message about screen rotation being unavailable.");
#else
            // Either iOS or Android.

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
                    CardPileGridPyramid.IsVisible = false;

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

            if (changedLayout)
            {
                SetLowerScrollViewOrientationLayout(isPortrait);

                var isGameCollectionViewBased = IsGameCollectionViewBased();

                SetUpperGridViewOrientationLayout(isPortrait);

                InnerMainGrid.IsVisible = true;

                CardPileGrid.IsVisible = isGameCollectionViewBased;
                CardPileGridPyramid.IsVisible = !isGameCollectionViewBased;

                NoScreenOrientationChangeLabel.IsVisible = false;
            }

            processingScreenOrientationChange = false;
        }

        private void SetUpperGridViewOrientationLayout(bool isPortrait)
        {
            if (isPortrait ||
                 (currentGameType == SolitaireGameType.Pyramid) ||
                 (currentGameType == SolitaireGameType.Tripeaks))
            {
                // First set the rows for the InnerMainGrid based on the current game.

                var rowDefinitionCollection = new RowDefinitionCollection();

                int upperGridRowSpan = (currentGameType != SolitaireGameType.Grandfathersclock ?
                                            2 : // Baker's Dozen
                                            7); // Grandfather's Clock

                for (int i = 0; i < 15; ++i)
                {
                    if ((currentGameType == SolitaireGameType.Klondike) &&
                        (i >= 10))
                    {
                        upperGridRowSpan = 3;

                        break;
                    }
                    else if ((currentGameType == SolitaireGameType.Spider) &&
                        (i >= 10))
                    {
                        upperGridRowSpan = 1;

                        break;
                    }
                    else if (((currentGameType == SolitaireGameType.Pyramid) ||
                              (currentGameType == SolitaireGameType.Tripeaks)) &&
                                (i >= 13))
                    {
                        upperGridRowSpan = 4;

                        break;
                    }

                    rowDefinitionCollection.Add( new RowDefinition(new GridLength(1, GridUnitType.Star)));
                }

                InnerMainGrid.RowDefinitions = rowDefinitionCollection;

                Debug.WriteLine("SetUpperGridViewOrientationLayout: Main area row count " + rowDefinitionCollection.Count);

                if (currentGameType == SolitaireGameType.Royalparade)
                {
                    upperGridRowSpan = 4;
                }

                Grid.SetRowSpan(UpperGrid, upperGridRowSpan);

                Grid.SetColumnSpan(TopCornerPiles, 2);

                if (currentGameType != SolitaireGameType.Grandfathersclock)
                {
                    var row = (currentGameType != SolitaireGameType.Bakersdozen ? 1 : 0);
                    Grid.SetRow(TargetPiles, row);

                    Grid.SetColumn(TargetPiles, 2);
                    Grid.SetColumnSpan(TargetPiles, 2);

                    var height = 0;

                    var vm = this.BindingContext as DealtCardViewModel;
                    if (vm != null)
                    {
                        if (InnerMainGrid.Height > 0)
                        {
                            if ((currentGameType == SolitaireGameType.Pyramid) ||
                                (currentGameType == SolitaireGameType.Tripeaks))
                            {
                                height = (int)(2 * InnerMainGrid.Height) / 9;
                            }
                            else
                            {
                                height = (int)(1 * InnerMainGrid.Height) / 9;

                                if (currentGameType == SolitaireGameType.Spider)
                                {
                                    SpiderDiscardedSequenceCountLabelContainer.HeightRequest = height;
                                    SpiderDiscardedSequenceCountLabel.HeightRequest = height;
                                }
                            }
                        }
                    }

                    if (height > 0)
                    { 
                        NextCardDeck.HeightRequest = height;

                        UpturnedCardsGrid.HeightRequest = height;

                        CardDeckUpturnedObscuredLower.HeightRequest = height;
                        CardDeckUpturnedObscuredHigher.HeightRequest = height;
                        CardDeckUpturned.HeightRequest = height;

                        TargetPileC.HeightRequest = height;
                        TargetPileD.HeightRequest = height;
                        TargetPileH.HeightRequest = height;
                        TargetPileS.HeightRequest = height;
                    }
                }
                else
                {
                    Grid.SetRow(TargetPilesClock, 0);
                    Grid.SetRowSpan(TargetPilesClock, 7);

                    Grid.SetColumn(TargetPilesClock, 3);
                }
            }
            else // Landscape.
            {
                // First set the rows for the InnerMainGrid based on the current game.

                var rowDefinitionCollection = new RowDefinitionCollection();

                for (int i = 0; i < 3; ++i)
                {
                    rowDefinitionCollection.Add(new RowDefinition(new GridLength(1, GridUnitType.Star)));
                }

                InnerMainGrid.RowDefinitions = rowDefinitionCollection;

                var upperGridRowSpan = (currentGameType != SolitaireGameType.Grandfathersclock ? 1 : 2);
                Grid.SetRowSpan(UpperGrid, upperGridRowSpan);

                Grid.SetColumnSpan(TopCornerPiles, 1);

                Grid.SetRow(TargetPiles, 0);
                Grid.SetColumn(TargetPiles, 3);
                Grid.SetColumnSpan(TargetPiles, 1);
            }
        }

        private void SetLowerScrollViewOrientationLayout(bool isPortrait)
        {
            int cardPileGridRow;
            int cardPileGridRowSpan;

            if (isPortrait ||
                 (currentGameType == SolitaireGameType.Pyramid) ||
                 (currentGameType == SolitaireGameType.Tripeaks))
            {
                switch (currentGameType)
                {
                    case SolitaireGameType.Spider:
                        cardPileGridRow = 2;
                        cardPileGridRowSpan = 10;
                        break;

                    case SolitaireGameType.Bakersdozen:
                        cardPileGridRow = 3;
                        cardPileGridRowSpan = 13;
                        break;

                    case SolitaireGameType.Grandfathersclock:
                        cardPileGridRow = 7;
                        cardPileGridRowSpan = 8;
                        break;

                    case SolitaireGameType.Pyramid:
                        cardPileGridRow = 4;
                        cardPileGridRowSpan = 9;
                        break;

                    case SolitaireGameType.Royalparade:
                        cardPileGridRow = 4;
                        cardPileGridRowSpan = 9;
                        break;

                    case SolitaireGameType.Tripeaks:
                        cardPileGridRow = 4;
                        cardPileGridRowSpan = 9;
                        break;

                    default: // Klondike:
                        cardPileGridRow = 3;
                        cardPileGridRowSpan = 7;
                        break;
                }
            }
            else // Landscape.
            {
                if (currentGameType != SolitaireGameType.Grandfathersclock)
                {
                    cardPileGridRow = 1;
                    cardPileGridRowSpan = 2;

                    if ((InnerMainGrid.Height > 0) && !isPortrait)
                    {
                        CardPileGrid.HeightRequest = (2 * InnerMainGrid.Height) / 3;
                    }
                }
                else
                {
                    cardPileGridRow = 2;
                    cardPileGridRowSpan = 1;

                    if ((InnerMainGrid.Height > 0) && !isPortrait)
                    {
                        CardPileGrid.HeightRequest = InnerMainGrid.Height / 3;
                    }
                }
            }

            if (IsGameCollectionViewBased())
            {
                Grid.SetRow(CardPileGrid, cardPileGridRow);
                Grid.SetRowSpan(CardPileGrid, cardPileGridRowSpan);

                SetCollectionViewsOrientationLayout(isPortrait);
            }
            else
            {
                Grid.SetRow(CardPileGridPyramid, cardPileGridRow);
                Grid.SetRowSpan(CardPileGridPyramid, cardPileGridRowSpan);
            }
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
            SetCollectionViewOrientationLayout(isPortrait, 7, CardPile8);
            SetCollectionViewOrientationLayout(isPortrait, 8, CardPile9);
            SetCollectionViewOrientationLayout(isPortrait, 9, CardPile10);
            SetCollectionViewOrientationLayout(isPortrait, 10, CardPile11);
            SetCollectionViewOrientationLayout(isPortrait, 11, CardPile12);
            SetCollectionViewOrientationLayout(isPortrait, 12, CardPile13);
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

                if ((MainPageGrid.Height > 0) && ((InnerMainGrid.Height > 0)))
                {
                    var vm = this.BindingContext as DealtCardViewModel;
                    if (vm != null)
                    {
                        if ((currentGameType == SolitaireGameType.Klondike) ||
                            (currentGameType == SolitaireGameType.Spider) ||
                            (currentGameType == SolitaireGameType.Bakersdozen))
                        {
                            var height = InnerMainGrid.Height / (3 + GetGameCardPileCount());

                            if (currentGameType == SolitaireGameType.Bakersdozen)
                            {
                                height -= 1;
                            }

                            collectionView.HeightRequest = height;
                        }
                        else if (currentGameType == SolitaireGameType.Grandfathersclock)
                        {
                            // Add some space between all the rows.
                            collectionView.HeightRequest = (InnerMainGrid.Height / 15) - 2;

                            collectionView.WidthRequest = InnerMainGrid.Width;
                        }
                    }
                }
            }
            else
            {
                Grid.SetRow(collectionView, 0);
                Grid.SetRowSpan(collectionView, 7);

                Grid.SetColumn(collectionView, index);
                Grid.SetColumnSpan(collectionView, 1);

                collectionView.ItemsLayout = LinearItemsLayout.Vertical;

                if ((MainPageGrid.Height > 0) && ((InnerMainGrid.Height > 0)))
                {
                    if (currentGameType != SolitaireGameType.Grandfathersclock)
                    {
                        collectionView.HeightRequest = (2 * InnerMainGrid.Height) / 3;
                    }
                    else
                    {
                        collectionView.HeightRequest = (1 * MainPageGrid.Height) / 3;
                    }
                }
            }
        }
    }
}
