using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;

namespace Sa11ytaire4All
{
    public partial class MainPage : ContentPage
    {
        private void ClearAllPiles()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            _deckRemaining.Clear();
            _deckUpturned.Clear();

            foreach (var targetPile in _targetPiles)
            {
                targetPile.Clear();
            }

            TargetPileC.Card = null;
            TargetPileD.Card = null;
            TargetPileH.Card = null;
            TargetPileS.Card = null;

            foreach (var dealtCardPile in vm.DealtCards)
            {
                dealtCardPile.Clear();
            }

            if (!IsGameCollectionViewBased())
            {
                ClearPyramidCardsSelection();
            }
        }

        private void RefreshUpperCards()
        {
            SetUpturnedCardsVisuals();

            var targetCardPileCount = _targetPiles[0].Count;
            TargetPileC.Card = (targetCardPileCount > 0 ? _targetPiles[0][targetCardPileCount - 1] : null);

            targetCardPileCount = _targetPiles[1].Count;
            TargetPileD.Card = (targetCardPileCount > 0 ? _targetPiles[1][targetCardPileCount - 1] : null);

            targetCardPileCount = _targetPiles[2].Count;
            TargetPileH.Card = (targetCardPileCount > 0 ? _targetPiles[2][targetCardPileCount - 1] : null);

            targetCardPileCount = _targetPiles[3].Count;
            TargetPileS.Card = (targetCardPileCount > 0 ? _targetPiles[3][targetCardPileCount - 1] : null);
        }

        private void ClearPyramidCardsSelection()
        {
            var pyramidCards = CardPileGridPyramid.Children;

            foreach (var pyramidCard in pyramidCards)
            {
                var card = pyramidCard as CardButton;
                if (card != null)
                {
                    card.IsToggled = false;
                }
            }
        }
    }
}
