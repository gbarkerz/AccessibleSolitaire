namespace Sa11ytaire4All
{
    // This file contains code relating to changing the layout of the app as sceen orientation changes.
    public partial class MainPage : ContentPage
    {
        private Timer? timerSetOrientationLayout;
        private Timer? timerSetCollectionViewsVisibility;

        private void Current_MainDisplayInfoChanged(object? sender, DisplayInfoChangedEventArgs e)
        {
            // The device screen has changed orientation, so update the layout of the UI accordingly.
            ChangeLayoutOrientation();
        }

        private void ChangeLayoutOrientation()
        {
            // We need to reset the cached initial dimensions here in order to recalculate the
            // new card size based on the new screen dimensions.
            OriginalCardWidth = 0;
            OriginalCardHeight = 0;

            // Barker Investigate: If we attempt to change the layout of all the visible UI now,
            // on iOS the app can hang. Presumably the considerable work being done to move 
            // many elements within their containing Grids, and having so much UI binding work
            // being done at the same time is problematic. So hide all the app's UI first, 
            // and after a brief delay relayout everything, and make the results visible. 
            // Tests seems to indicate that both Android and iOS is happy with this approach.

            timerSetOrientationLayout = new Timer(
                new TimerCallback((s) => DelayedSetOrientationLayout()),
                    null,
                    TimeSpan.FromMilliseconds(500),
                    TimeSpan.FromMilliseconds(Timeout.Infinite));

            // Note: On iOS, sometimes when the LowerScrollView is made visible later,
            // one of the contained CollectionViews does not reappear. So in an attempt
            // to reduce the chances of that happenening expicitly set the visibility
            // of the CollectionViews.
            SetCollectionViewsVisibility(false);

            UpperGrid.IsVisible = false;
            CardPileGrid.IsVisible = false;
        }

        public void DelayedSetOrientationLayout()
        {
            timerSetOrientationLayout?.Dispose();
            timerSetOrientationLayout = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SetOrientationLayout();

                UpperGrid.IsVisible = true;
                CardPileGrid.IsVisible = true;

                // Actually, on Android when moving from portrait to landscape, sometimes the width
                // of the CollectionViews is too narrow. So delay making the CollectionViews visible
                // until their containers have had a chance to react to their new dimensions.
                DelaySetCollectionViewsVisibility(true);
            });
        }

        private void DelaySetCollectionViewsVisibility(bool visible)
        {
            timerSetCollectionViewsVisibility = new Timer(
                new TimerCallback((s) => DelayedSetCollectionViewsVisibility(visible)),
                    null,
                    TimeSpan.FromMilliseconds(500),
                    TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        private void DelayedSetCollectionViewsVisibility(bool visible)
        {
            timerSetCollectionViewsVisibility?.Dispose();
            timerSetCollectionViewsVisibility = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                SetCollectionViewsVisibility(visible);
            });
        }

        private void SetCollectionViewsVisibility(bool visible)
        {
            CardPile1.IsVisible = visible;
            CardPile2.IsVisible = visible;
            CardPile3.IsVisible = visible;
            CardPile4.IsVisible = visible;
            CardPile5.IsVisible = visible;
            CardPile6.IsVisible = visible;
            CardPile7.IsVisible = visible;
        }

        private void SetOrientationLayout()
        {
            var isPortrait = MainPage.IsPortrait();

            SetUpperGridViewOrientationLayout(isPortrait);

            SetLowerScrollViewOrientationLayout(isPortrait);
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
            if (isPortrait)
            {
                Grid.SetRow(CardPileGrid, 2);
                Grid.SetRowSpan(CardPileGrid, 7);
            }
            else
            {
                Grid.SetRow(CardPileGrid, 3);
                Grid.SetRowSpan(CardPileGrid, 6);
            }

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