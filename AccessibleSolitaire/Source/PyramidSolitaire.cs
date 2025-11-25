using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;
using System.Diagnostics;

namespace Sa11ytaire4All
{
    public enum SolitaireGameType
    {
        NoGame = 0,
        Klondike = 1,
        Pyramid = 2
    }

    public partial class MainPage : ContentPage
    {
        static public SolitaireGameType currentGameType;

        public void LoadKlondikeGame()
        {
            ChangeGameType(SolitaireGameType.Klondike);
        }

        public void LoadPyramidGame()
        {
            ChangeGameType(SolitaireGameType.Pyramid);
        }

        private void ChangeGameType(SolitaireGameType targetGameType)
        {
            SaveSession();

            // Barker Todo: Remove currentGameType now that we have vm.CurrentGameType.
            currentGameType = targetGameType;

            Preferences.Set("CurrentGameType", Convert.ToInt32(currentGameType));

            var vm = this.BindingContext as DealtCardViewModel;
            if (vm != null)
            {
                vm.CurrentGameType = currentGameType;
            }

            var isKlondike = (currentGameType == SolitaireGameType.Klondike);

            CardDeckUpturnedObscuredLower.IsVisible = isKlondike;

            TargetPiles.IsVisible = isKlondike;

            ClearAllPiles();

            if (!LoadSession())
            {
                RestartGame(true /* screenReaderAnnouncement. */);
            }
        }

        private void AddPyramidButtons()
        {
            CardButton button;

            var countOfCardsPerRow = 1;
            var countOfCardsOnCurrentRow = 0;

            var startColumn = 6;
            var currentColumn = 6;

            var vm = this.BindingContext as DealtCardViewModel;

            var setSemanticHeading = true;

            for (int i = 0; i < 28; i++)
            {
                button = new CardButton();

                button.Margin = new Thickness(20, 0, 20, 0);
                button.Padding = new Thickness(0);

                button.IsToggled = false;

                button.BackgroundColor = Colors.White;

                CardPileGridPyramid.Children.Add(button);

                CardPileGridPyramid.SetRow(button, countOfCardsPerRow - 1);
                CardPileGridPyramid.SetRowSpan(button, 3);

                CardPileGridPyramid.SetColumn(button, currentColumn);
                CardPileGridPyramid.SetColumnSpan(button, 2);

                if (setSemanticHeading)
                {
                    button.SetHeadingState(true);

                    setSemanticHeading = false;
                }

                ++countOfCardsOnCurrentRow;

                currentColumn += 2;

                if (countOfCardsOnCurrentRow == countOfCardsPerRow)
                {
                    ++countOfCardsPerRow;

                    countOfCardsOnCurrentRow = 0;

                    --startColumn;

                    currentColumn = startColumn;

                    setSemanticHeading = true;
                }
            }
        }

        private void DealPyramidCardsPostprocess(bool setDealtCardProperties)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            vm.PyramidCardDiscarded = false;

            var cardButtonsUI = CardPileGridPyramid.Children;

            var countCardsPerRow = 1;

            var cardUIIndex = 0;

            var totalCountOfVisibleCardsInPyramid = 0;

            for (int i = 0; i < 7; ++i)
            {
                var currentVisibleIndexOfCardOnRow = 0;

                // How many cards are currently shows on this row?
                var countOfVisibleCardsOnRow = 0;

                for (int j = 0; j < countCardsPerRow; ++j)
                {
                    var dealtCard = vm.DealtCards[i][j];
                    if (dealtCard.Card != null)
                    {
                        ++countOfVisibleCardsOnRow;
                    }
                }

                totalCountOfVisibleCardsInPyramid += countOfVisibleCardsOnRow;

                for (int j = 0; j < countCardsPerRow; ++j)
                {
                    var dealtCard = vm.DealtCards[i][j];

                    // All of these are zero-based.
                    dealtCard.PyramidRow = i;
                    dealtCard.PyramidCardOriginalIndexInRow = j;
                    dealtCard.PyramidCardCurrentCountOfCardsOnRow = countOfVisibleCardsOnRow;

                    dealtCard.PyramidCardCurrentIndexInRow = currentVisibleIndexOfCardOnRow;

                    if (dealtCard.Card != null)
                    {
                        ++currentVisibleIndexOfCardOnRow;
                    }

                    if (setDealtCardProperties)
                    {
                        dealtCard.Open = (dealtCard.PyramidRow == 6);
                    }

                    var cardUI = cardButtonsUI[cardUIIndex] as CardButton;
                    if (cardUI != null)
                    {
                        var cardButtonIsVisible = (dealtCard.Card != null);

                        cardUI.IsVisible = cardButtonIsVisible;

                        cardUI.Card = dealtCard.Card;
                    }

                    ++cardUIIndex;
                }

                ++countCardsPerRow;
            }

            vm.PyramidCardDiscarded = (totalCountOfVisibleCardsInPyramid < 28);
        }

        // Handle a click on one of the CardButtons in the pyramd.
        private void HandlePyramidCardClick(CardButton cardButtonClicked)
        {
            if (cardButtonClicked.Card == null)
            {
                return;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // Track whether we'll be removing the card from the pyramid.
            var removeCard = false;

            // First check whether the main upturned card is selected and totals 13.
            if (CardDeckUpturned.IsToggled && (CardDeckUpturned.Card != null))
            {
                // Remove the selected Upturned card if appropriate.
                removeCard = CanRemoveUpturnedCardAndPyramidCard(CardDeckUpturned, cardButtonClicked);
                if (removeCard)
                {
                    // There is now currently no Upturned card.
                    CardDeckUpturned.Card = null;

                    CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 1 ?
                                        _deckUpturned[_deckUpturned.Count - 1] : null);
                }
            }

            // If we didn't remove the Upturned card, check the top ofthe Waste pile (ie the higher obscured card).
            if (!removeCard)
            {
                if (CardDeckUpturnedObscuredHigher.IsToggled && (CardDeckUpturnedObscuredHigher.Card != null))
                {
                    removeCard = CanRemoveUpturnedCardAndPyramidCard(CardDeckUpturnedObscuredHigher, cardButtonClicked);
                }
            }

            CardButton? cardAlreadySelected = null;

            // If we've not removed the Upturned card or higher Obscured card, check the rest of the pyramid.
            if (!removeCard)
            {
                // Is any card in the pyramid already selected?
                var gridCards = CardPileGridPyramid.Children;

                foreach (var gridCard in gridCards)
                {
                    var card = gridCard as CardButton;
                    if ((card != null) && (card != cardButtonClicked) && card.IsToggled)
                    {
                        cardAlreadySelected = card;

                        break;
                    }
                }

                // Was another pyramid card already selected?
                if ((cardAlreadySelected != null) && (cardAlreadySelected.Card != null))
                {
                    // Do the selected pyramid cards add up to 13?
                    var total = cardButtonClicked.Card.Rank + cardAlreadySelected.Card.Rank;
                    if (total == 13)
                    {
                        // Remove the already selected card.
                        CollectionView? list;
                        var dealtCardAlreadySelected = FindDealtCardFromCard(cardAlreadySelected.Card, false, out list);
                        if (dealtCardAlreadySelected != null)
                        {
                            // Has a card now been revealed? 
                            SetOnTopStateFollowingMove(dealtCardAlreadySelected);

                            RefreshCardButtonMofNInRow(cardAlreadySelected);

                            // Always null out this card after the call to refresh the accessible name.
                            vm.DealtCards[dealtCardAlreadySelected.PyramidRow]
                                [dealtCardAlreadySelected.PyramidCardOriginalIndexInRow].Card = null;
                        }

                        cardAlreadySelected.IsVisible = false;
                        cardAlreadySelected.IsToggled = false;

                        removeCard = true;
                    }
                }
                else if (cardButtonClicked.Card.Rank == 13)
                {
                    // If the clicked card is simply a King, remove it.
                    removeCard = true;
                }
            }

            // Should we remove the clicked pyramid card?
            if (removeCard)
            {
                CollectionView? list;
                var dealtCard = FindDealtCardFromCard(cardButtonClicked.Card, false, out list);
                if (dealtCard != null)
                {
                    // Has a card now been revealed? 
                    SetOnTopStateFollowingMove(dealtCard);

                    RefreshCardButtonMofNInRow(cardButtonClicked);

                    vm.DealtCards[dealtCard.PyramidRow][dealtCard.PyramidCardOriginalIndexInRow].Card = null;

                    cardButtonClicked.IsVisible = false;
                    cardButtonClicked.IsToggled = false;

                    SetDiscardedPileUIState();
                }
            }
            else
            {
                // The clicked card was not removed. If another pyramid card was already selected, deselect it.
                if (cardAlreadySelected != null)
                {
                    cardButtonClicked.IsToggled = false;

                    cardAlreadySelected.IsToggled = false;
                }
            }

            if (GameOver())
            {
                ShowEndOfGameDialog();
            }
        }

        private void HandleClickOnUpturnedOrHigherObscuredCard(CardButton cardDeckUpturned)
        {
            // Is the most recently upturned card a King?
            if (cardDeckUpturned.Card != null)
            {
                if (cardDeckUpturned.Card.Rank == 13)
                {
                    // Move the King to the discard pile.
                    RemoveCardButtonCard(_deckUpturned, cardDeckUpturned.Card);

                    // The most recently upturned card is now empty.
                    if (cardDeckUpturned == CardDeckUpturned)
                    {
                        CardDeckUpturned.Card = null;
                    }
                    else
                    {
                        // The top of the other pile is the next most recently upturned card.
                        CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 1 ?
                                            _deckUpturned[_deckUpturned.Count - 2] : null);
                    }
                }
                else
                {
                    // The upturned card is not a King. Check it's a pair with an already selected pyramid card.
                    var vm = this.BindingContext as DealtCardViewModel;
                    if ((vm != null) && (vm.DealtCards != null))
                    {
                        CardButton? cardAlreadySelected = null;

                        // Is any card in the pyramid already selected?
                        var gridCards = CardPileGridPyramid.Children;

                        foreach (var gridCard in gridCards)
                        {
                            var card = gridCard as CardButton;
                            if ((card != null) && card.IsToggled)
                            {
                                cardAlreadySelected = card;

                                break;
                            }
                        }

                        var toggleSelectionStateOfUpturnedCard = false;

                        if ((cardAlreadySelected != null) && ((cardAlreadySelected.Card != null)))
                        {
                            var removeCard = CanRemoveUpturnedCardAndPyramidCard(cardDeckUpturned, cardAlreadySelected);
                            if (removeCard)
                            {
                                CollectionView? list;
                                var dealtCardAlreadySelected = FindDealtCardFromCard(cardAlreadySelected.Card, false, out list);
                                if (dealtCardAlreadySelected != null)
                                {
                                    // Has a card now been revealed? 
                                    SetOnTopStateFollowingMove(dealtCardAlreadySelected);

                                    RefreshCardButtonMofNInRow(cardAlreadySelected);

                                    vm.DealtCards[dealtCardAlreadySelected.PyramidRow]
                                        [dealtCardAlreadySelected.PyramidCardOriginalIndexInRow].Card = null;

                                    cardAlreadySelected.IsVisible = false;
                                    cardAlreadySelected.IsToggled = false;

                                    // Make sure the Upturned card appears as not selected.
                                    cardDeckUpturned.RefreshVisuals();
                                }

                                // The most recently upturned card is now empty.
                                if (cardDeckUpturned == CardDeckUpturned)
                                {
                                    CardDeckUpturned.Card = null;

                                    CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 0 ?
                                                        _deckUpturned[_deckUpturned.Count - 1] : null);
                                }
                                else
                                {
                                    // The top of the other pile is the next most recently upturned card.
                                    CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 1 ?
                                                        _deckUpturned[_deckUpturned.Count - 2] : null);
                                }
                            }
                            else
                            {
                                toggleSelectionStateOfUpturnedCard = true;
                            }
                        }
                        else
                        {
                            toggleSelectionStateOfUpturnedCard = true;
                        }

                        if (toggleSelectionStateOfUpturnedCard)
                        {
                            // Toggle the selected state of the upturned card.
                            ToggleUpturnedCardSelection(cardDeckUpturned);
                        }
                    }
                }
            }
        }

        // A pyramid card has been clicked. Remove the Upturned card if appropriate.
        private bool CanRemoveUpturnedCardAndPyramidCard(CardButton upturnedCard, CardButton cardButton)
        {
            if ((upturnedCard.Card == null) || (cardButton.Card == null))
            {
                return false;
            }

            bool canRemoveUpturnedCardAndPyramidCard = false;

            if (upturnedCard.Card.Rank + cardButton.Card.Rank == 13)
            {
                RemoveCardButtonCard(_deckUpturned, upturnedCard.Card);

                SetUpturnedCardsVisuals();

                upturnedCard.IsToggled = false;

                canRemoveUpturnedCardAndPyramidCard = true;
            }
            else
            {
                // Deselect the two cards because they don't total 13.
                upturnedCard.IsToggled = false;
                cardButton.IsToggled = false;
            }

            return canRemoveUpturnedCardAndPyramidCard;
        }

        // Set the property to indicate the Discard pile is not empty.
        private void RemoveCardButtonCard(List<Card> cardList, Card cardToRemove)
        {
            cardList.Remove(cardToRemove);

            SetDiscardedPileUIState();
        }

        private void SetDiscardedPileUIState()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                vm.PyramidCardDiscarded = true;
            }
        }

        // Check if both the Upturned card and HigherObscured card should be removed.
        private bool MoveBothUpturnedCards(CardButton cardButtonClicked)
        {
            if ((CardDeckUpturned.Card == null) || (CardDeckUpturnedObscuredHigher.Card == null))
            {
                return false;
            }

            var continueCheckForMove = true;

            var checkForMove = ((cardButtonClicked == CardDeckUpturned) && CardDeckUpturnedObscuredHigher.IsToggled) ||
                               ((cardButtonClicked == CardDeckUpturnedObscuredHigher) && CardDeckUpturned.IsToggled);

            if (checkForMove)
            {
                if (CardDeckUpturned.Card.Rank + CardDeckUpturnedObscuredHigher.Card.Rank == 13)
                {
                    RemoveCardButtonCard(_deckUpturned, CardDeckUpturned.Card);
                    RemoveCardButtonCard(_deckUpturned, CardDeckUpturnedObscuredHigher.Card);

                    CardDeckUpturned.Card = null;

                    CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 0 ?
                                        _deckUpturned[_deckUpturned.Count - 1] : null);

                    continueCheckForMove = false;

                    // Make sure the cards appear as not selected.
                    if (cardButtonClicked != CardDeckUpturned)
                    {
                        CardDeckUpturned.IsToggled = false;
                        CardDeckUpturned.RefreshVisuals();
                    }

                    if (cardButtonClicked != CardDeckUpturnedObscuredHigher)
                    {
                        CardDeckUpturnedObscuredHigher.IsToggled = false;
                        CardDeckUpturnedObscuredHigher.RefreshVisuals();
                    }
                }
            }

            return continueCheckForMove;
        }

        private void RefreshAccessibleCardCountInRow(CardButton cardButton, int pyramidRow)
        {
            if (cardButton.Card == null)
            {
                return;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            var cardButtonsUI = CardPileGridPyramid.Children;
            for (int i = 0; i < cardButtonsUI.Count; ++i)
            {
                // Refresh all the CardButton UI on the affected row.
                var cardButtonUIRowStartIndex = GetCardButtonUIStartIndexForRow(pyramidRow);

                for (int rowIndex = 0; rowIndex < pyramidRow + 1; ++rowIndex)
                {
                    var nextCardButton = cardButtonsUI[cardButtonUIRowStartIndex + rowIndex] as CardButton;
                    if ((nextCardButton != null) && nextCardButton.IsVisible)
                    {
                        nextCardButton.RefreshCardButtonMofN();
                    }
                }
            }
        }

        // Adjust the M of N for all cards on a row.
        private void RefreshCardButtonMofNInRow(CardButton cardButton)
        {
            if (cardButton.Card == null)
            {
                return;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            CollectionView? list;
            var dealtCard = FindDealtCardFromCard(cardButton.Card, false, out list);
            if ((dealtCard != null) && ((dealtCard.Card != null)))
            {
                var cardButtonsUI = CardPileGridPyramid.Children;
                for (int i = 0; i < cardButtonsUI.Count; ++i)
                {
                    var cardButtonPyramid = cardButtonsUI[i] as CardButton;
                    if ((cardButtonPyramid != null) && (cardButtonPyramid.Card != null) &&
                        (cardButtonPyramid.Card == cardButton.Card))
                    {
                        --dealtCard.PyramidCardCurrentCountOfCardsOnRow;

                        var dealtCardsInRow = vm.DealtCards[dealtCard.PyramidRow];
                        if (dealtCardsInRow != null)
                        {
                            for (int j = 0; j < dealtCardsInRow.Count; ++j)
                            {
                                var dealtCardInRow = dealtCardsInRow[j];
                                if ((dealtCardInRow != null) && (dealtCardInRow != dealtCard))
                                {
                                    if (j > dealtCard.PyramidCardOriginalIndexInRow)
                                    {
                                        --dealtCardInRow.PyramidCardCurrentIndexInRow;
                                    }

                                    dealtCardInRow.PyramidCardCurrentCountOfCardsOnRow = dealtCard.PyramidCardCurrentCountOfCardsOnRow;
                                }
                            }
                        }

                        break;
                    }
                }

                RefreshAccessibleCardCountInRow(cardButton, dealtCard.PyramidRow);
            }
        }

        private int GetCardButtonUIStartIndexForRow(int pyramidRow)
        {
            var startIndex = 0;

            var rowCount = 1;

            for (int i = 0; i < pyramidRow; ++i)
            {
                startIndex += rowCount;

                ++rowCount;
            }

            return startIndex;
        }

        private void SetOnTopStateFollowingMove(DealtCard dealtCard)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            var rowRevealed = dealtCard.PyramidRow - 1;
            if (rowRevealed >= 0)
            {
                // First check whether the card above left is now revealed.
                if (dealtCard.PyramidCardOriginalIndexInRow > 0)
                {
                    var cardToLeft = vm.DealtCards[dealtCard.PyramidRow][dealtCard.PyramidCardOriginalIndexInRow - 1];
                    if (cardToLeft.Card == null)
                    {
                        var cardAboveToLeft = vm.DealtCards[rowRevealed][dealtCard.PyramidCardOriginalIndexInRow - 1] as DealtCard;
                        cardAboveToLeft.Open = true;

                        var cardButtonUI = FindPyramidCardButtonFromDealtCard(cardAboveToLeft);
                        if (cardButtonUI != null)
                        {
                            cardButtonUI.RefreshCardButtonMofN();
                        }
                    }
                }

                // Next check whether the card above right is now revealed.
                if (dealtCard.PyramidCardOriginalIndexInRow < dealtCard.PyramidRow)
                {
                    var cardToRight = vm.DealtCards[dealtCard.PyramidRow][dealtCard.PyramidCardOriginalIndexInRow + 1];
                    if (cardToRight.Card == null)
                    {
                        var cardAboveToRight = vm.DealtCards[rowRevealed][dealtCard.PyramidCardOriginalIndexInRow] as DealtCard;
                        cardAboveToRight.Open = true;

                        var cardButtonUI = FindPyramidCardButtonFromDealtCard(cardAboveToRight);
                        if (cardButtonUI != null)
                        {
                            cardButtonUI.RefreshCardButtonMofN();
                        }
                    }
                }
            }
        }

        private CardButton? FindPyramidCardButtonFromDealtCard(DealtCard dealtCard)
        {
            CardButton? foundCardButton = null;

            if (dealtCard.Card != null)
            {
                var cardButtonsUI = CardPileGridPyramid.Children;
                for (int i = 0; i < cardButtonsUI.Count; ++i)
                {
                    var cardButtonPyramid = cardButtonsUI[i] as CardButton;
                    if ((cardButtonPyramid != null) && (cardButtonPyramid.Card != null) &&
                        (cardButtonPyramid.Card == dealtCard.Card))
                    {
                        foundCardButton = cardButtonPyramid;

                        break;
                    }
                }
            }

            return foundCardButton;
        }
    }
}