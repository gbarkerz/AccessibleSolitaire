
namespace Sa11ytaire4All
{
    // This file contains code relating to changing the layout of the app as sceen orientation changes.
    public partial class MainPage : ContentPage
    {
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
