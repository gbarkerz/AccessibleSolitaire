using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

namespace Sa11ytaire4All
{
    public partial class MainPage : ContentPage
    {
        private void PersistRoyalParadeDealtCardsRowFour(string preferenceSuffix)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // Get all the cards (up to 8 piles of 9 cards) on the bottow row.
            var dealtCardPileBottomRow = vm.DealtCards[3].ToArray();

            var index = 0;

            var dealtCardsBottomRowSlot = new DealtCard?[8];

            // Process each of the nine horizontal slots of the eight piles of cards on the bottom row.
            for (var bottowRowSlotIndex = 0; bottowRowSlotIndex < 9; ++bottowRowSlotIndex)
            {
                // Now output each card of this particular horizontal slot, (some slots may be null).
                for (var pileIndex = 0; pileIndex < 8; ++pileIndex)
                {
                    var dealtCard = dealtCardPileBottomRow[pileIndex + index];
                    if (dealtCard != null)
                    {
                        // We'll be outputting this dealt card. The dealt card's CurrentDealtCardPileIndex
                        // indicates which pile along the horizontal slot it belongs in.
                        dealtCardsBottomRowSlot[pileIndex] = dealtCardPileBottomRow[pileIndex + index];
                    }
                    else
                    {
                        dealtCardsBottomRowSlot[pileIndex] = null;
                    }
                }

                var dealtCardPileBottomRowJson = JsonSerializer.Serialize(dealtCardsBottomRowSlot);

                Preferences.Set("DealtCardsSessionBottomRow" + bottowRowSlotIndex.ToString() +
                    preferenceSuffix, dealtCardPileBottomRowJson);

                // Move up to the next horiztonal slot.
                index += 8;
            }
        }

        private void LoadRoyalParadeDealtCardsRowFour(string preferenceSuffix)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // Process each of the nine horizontal slots of the eight piles of cards on the bottom row.
            for (var bottowRowSlotIndex = 0; bottowRowSlotIndex < 9; ++bottowRowSlotIndex)
            {
                // Load each card of this particular horizontal slot, (and the card can ne null).
                var dealtCardPileBottomRowJson = (string)Preferences.Get("DealtCardsSessionBottomRow" + 
                                                    bottowRowSlotIndex.ToString() + preferenceSuffix, "");
                if (!string.IsNullOrEmpty(dealtCardPileBottomRowJson))
                {
                    // Get an array of DealtCards for this horizontal slot.
                    var dealtCardPile = JsonSerializer.Deserialize<ObservableCollection<DealtCard>>(dealtCardPileBottomRowJson);
                    if (dealtCardPile != null)
                    {
                        // Now add each of the loaded DealtCards to the full collection of bottom row cards.
                        foreach (var dealtCard in dealtCardPile)
                        {
                            // dealtCard can be null here.
                            vm.DealtCards[3].Add(dealtCard);
                        }
                    }
                }
            }
        }

        private void RoyalParadeMoveCardToEmptySlot(
            CardButton cardButtonDestination,
            CardButton cardButtonToMove,
            DealtCard dealtCardToMove)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            var cardButtonDestinationRow = -1;
            var cardButtonDestinationColumn = -1;

            var gridCards = CardPileGridPyramid.Children;

            // Find the row and column of the destination slot.
            for (var i = 0; i < gridCards.Count; ++i)
            {
                var cardButton = gridCards[i] as CardButton;
                if (cardButton == cardButtonDestination)
                {
                    cardButtonDestinationRow = i / 8;
                    cardButtonDestinationColumn = i % 8;

                    // Check that the card being moved is a match for the destination row.
                    if ((cardButtonToMove.Card != null) &&
                        (((cardButtonToMove.Card.Rank == 2) && (cardButtonDestinationRow == 0)) ||
                            ((cardButtonToMove.Card.Rank == 3) && (cardButtonDestinationRow == 1)) ||
                            ((cardButtonToMove.Card.Rank == 4) && (cardButtonDestinationRow == 2))))
                    {
                        // Deal with the StackDetails before nulling our any card.
                        if ((vm.DealtCards[cardButtonDestinationRow][cardButtonDestinationColumn] != null) &&
                            (vm.DealtCards[dealtCardToMove.PyramidRow]
                                                    [dealtCardToMove.PyramidCardOriginalIndexInRow] != null))
                        {
                            if (vm.DealtCards[dealtCardToMove.PyramidRow][dealtCardToMove.PyramidCardOriginalIndexInRow].Card != null)
                            {
                                vm.DealtCards[cardButtonDestinationRow][cardButtonDestinationColumn].StackDetails =
                                        vm.DealtCards[dealtCardToMove.PyramidRow]
                                            [dealtCardToMove.PyramidCardOriginalIndexInRow].Card.Rank.ToString();
                            }

                            cardButtonDestination.StackDetails =
                                vm.DealtCards[cardButtonDestinationRow][cardButtonDestinationColumn].StackDetails;
                        }

                        if (vm.DealtCards[dealtCardToMove.PyramidRow]
                            [dealtCardToMove.PyramidCardOriginalIndexInRow] != null)
                        {
                            vm.DealtCards[dealtCardToMove.PyramidRow]
                                [dealtCardToMove.PyramidCardOriginalIndexInRow].StackDetails = "";
                            cardButtonToMove.StackDetails = "";
                        }

                        // Set the destination card to be the same as the card being moved.
                        vm.DealtCards[cardButtonDestinationRow][cardButtonDestinationColumn].Card =
                                vm.DealtCards[dealtCardToMove.PyramidRow]
                                    [dealtCardToMove.PyramidCardOriginalIndexInRow].Card;

                        // If the card being moved is on the first 3 rows, set its contained card to null.
                        // If it's on the 4th row, null out the entire slot.
                        if (dealtCardToMove.PyramidRow < 3)
                        {
                            vm.DealtCards[dealtCardToMove.PyramidRow]
                                [dealtCardToMove.PyramidCardOriginalIndexInRow].Card = null;
                        }
                        else
                        {
                            vm.DealtCards[dealtCardToMove.PyramidRow]
                                [dealtCardToMove.PyramidCardOriginalIndexInRow] = null;
                        }

                        // Set the desintation card on the visual CardButton.
                        cardButtonDestination.Card = cardButtonToMove.Card;

                        // Until we know otherwise, the CardButton being moved now has a null contained card.
                        cardButtonToMove.Card = null;

                        // If the card being moved is on the 4th row, we might now show the card that
                        // was previously beneath the card being moved.
                        if (dealtCardToMove.PyramidRow == 3)
                        {
                            // Work downwards from the highest slot in this 4th row pile.
                            for (var j = dealtCardToMove.PyramidCardOriginalIndexInRow - 8; j > 0; j -= 8)
                            {
                                // Is there a card now exposed?
                                var dealtCardBelow = vm.DealtCards[3][j];
                                if ((dealtCardBelow != null) && (dealtCardBelow.Card != null))
                                {
                                    // Show the exposed card in the CardButton now.
                                    cardButtonToMove.Card = dealtCardBelow.Card;

                                    cardButtonToMove.StackDetails = dealtCardBelow.StackDetails;

                                    break;
                                }
                            }
                        }

                        // The CardButton for the card being moved is visible unless this bottom row pile
                        // is now empty.
                        cardButtonToMove.IsVisible = (dealtCardToMove.PyramidRow < 3) ||
                                    (vm.DealtCards[dealtCardToMove.PyramidRow]
                                        [dealtCardToMove.PyramidCardOriginalIndexInRow % 8] != null);

                        // If any card now occupies the moved card slot, it's not open.
                        if (vm.DealtCards[dealtCardToMove.PyramidRow]
                            [dealtCardToMove.PyramidCardOriginalIndexInRow] != null)
                        {
                            vm.DealtCards[dealtCardToMove.PyramidRow]
                                [dealtCardToMove.PyramidCardOriginalIndexInRow].Open = false;

                            cardButtonToMove.Open = false;
                        }

                        // The destination slot is always open and has one card in it.
                        vm.DealtCards[cardButtonDestinationRow][cardButtonDestinationColumn].Open = true;
                        cardButtonDestination.Open = true;

                        cardButtonDestination.RefreshVisuals();
                        cardButtonToMove.RefreshVisuals();
                    }

                    break;
                }
            }
        }

        private void SetRoyalParadeCardButtonEmpty(CardButton cardButton)
        {
            cardButton.IsVisible = false;
            cardButton.Card = null;
            cardButton.Open = false;
            cardButton.StackDetails = "";
        }

        private void PrepareCardsForRoyalParade()
        {
            // Leave any Aces in place until removed by the player.
            //RemoveAcesFromRemainingCards();

            Move234ToStartOfRow();
        }

        //private void RemoveAcesFromRemainingCards()
        //{
        //    // Find and remove all of the Aces in the cards.
        //    for (int i = _deckRemaining.Count - 1; i >= 0; --i)
        //    {
        //        if ((_deckRemaining[i] != null) && (_deckRemaining[i].Rank == 1))
        //        {
        //            _deckRemaining.RemoveAt(i);
        //        }
        //    }
        //}

        private void Move234ToStartOfRow()
        {
            var index2 = -1;
            var index3 = -1;
            var index4 = -1;

            // Find a 2, a 3 and a 4.
            for (int i = _deckRemaining.Count - 1; i >= 0; --i)
            {
                if (_deckRemaining[i] != null)
                {
                    if ((index2 == -1) && (_deckRemaining[i].Rank == 2))
                    {
                        index2 = i;
                    }
                    else if ((index3 == -1) && (_deckRemaining[i].Rank == 3))
                    {
                        index3 = i;
                    }
                    else if ((index4 == -1) && (_deckRemaining[i].Rank == 4))
                    {
                        index4 = i;
                    }

                    if ((index2 != -1) &&
                        (index3 != -1) &&
                        (index4 != -1))
                    {
                        break;
                    }
                }
            }

            // Now if necessary, swap a 2, a 3, and a 4, such that they lie at
            // the start of the first, second, and third rows respectively. 

            if (_deckRemaining[0].Rank != 2)
            {
                SwapCard(0, index2);
            }

            if (_deckRemaining[8].Rank != 3)
            {
                SwapCard(8, index3);
            }

            if (_deckRemaining[16].Rank != 4)
            {
                SwapCard(16, index4);
            }
        }

        private void SwapCard(int indexCardFirst, int indexCardSecond)
        {
            Card tempCard = new Card();
            tempCard.Suit = _deckRemaining[indexCardFirst].Suit;
            tempCard.Rank = _deckRemaining[indexCardFirst].Rank;

            _deckRemaining[indexCardFirst].Suit = _deckRemaining[indexCardSecond].Suit;
            _deckRemaining[indexCardFirst].Rank = _deckRemaining[indexCardSecond].Rank;

            _deckRemaining[indexCardSecond].Suit = tempCard.Suit;
            _deckRemaining[indexCardSecond].Rank = tempCard.Rank;
        }

        private void CheckForOpenCardsRoyalParade()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            var gridCards = CardPileGridPyramid.Children;

            for (var i = 0; i < 3; ++i)
            {
                if (vm.DealtCards[i] != null)
                {
                    for (var j = 0; j < vm.DealtCards[i].Count; ++j)
                    {
                        var dealtCard = vm.DealtCards[i][j];
                        if (dealtCard != null)
                        {
                            var card = dealtCard.Card;
                            if (card != null)
                            {
                                var cardIsOpen = (card.Rank == i + 2);

                                dealtCard.Open = cardIsOpen;

                                var cardUIIndex = (i * 8) + j;
                                var cardButton = gridCards[cardUIIndex] as CardButton;
                                if (cardButton != null)
                                {
                                    cardButton.Open = cardIsOpen;
                                }
                            }
                        }
                    }
                }
            }

            if (currentGameType == SolitaireGameType.Royalparade)
            {
                RefreshAllCardVisuals();
            }
        }

        private CardButton? GetAlreadySelectedCard(CardButton? ignoreCardButton)
        {
            CardButton? cardAlreadySelected = null;

            // Is any card in the pyramid already selected?
            var gridCards = CardPileGridPyramid.Children;

            foreach (var gridCard in gridCards)
            {
                var card = gridCard as CardButton;
                if ((card != null) && (ignoreCardButton != null) && 
                    (card != ignoreCardButton) && card.IsToggled)
                {
                    cardAlreadySelected = card;

                    break;
                }
            }

            return cardAlreadySelected;
        }

        // Handle a click on any card in the Royal Parade game.
        private async void HandleRoyalParadePyramidCardClick(CardButton cardButtonClicked)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            var moveCard = false;

            // Was another pyramid card already selected?
            CardButton? cardAlreadySelected = GetAlreadySelectedCard(cardButtonClicked);
            if ((cardAlreadySelected != null) && (cardAlreadySelected.Card != null))
            {
                // A card is already selected. If it's Open, then it cannot be moved.
                if (!cardAlreadySelected.Open)
                {
                    // Has an empty slot been clicked in the first three rows been clicked?
                    if (cardButtonClicked.Card == null)
                    {
                        // Yes, so we can attempt to move the clicked card to it.
                        moveCard = true;
                    }
                    else
                    {
                        // The clicked card is not empty. Can the already selected card be moved to clicked card?
                        // Royal Parade expects the cards to be the same suit.
                        var rankDifference = cardAlreadySelected.Card.Rank - cardButtonClicked.Card.Rank;
                        if ((rankDifference == 3) && cardButtonClicked.Open &&
                            (cardButtonClicked.Card.Suit == cardAlreadySelected.Card.Suit))
                        {
                            moveCard = true;
                        }
                    }
                }

                // Can we consider moving a card?
                if (moveCard)
                {
                    // Get the dealt cards associated with the already-selected card and the clicked card.
                    CollectionView? list;
                    var dealtCardAlreadySelected = FindDealtCardFromCard(cardAlreadySelected.Card, false, out list);
                    if (dealtCardAlreadySelected != null)
                    {
                        // Is the clicked card an empty spot?
                        if (cardButtonClicked.Card != null)
                        {
                            var dealtCardClicked = FindDealtCardFromCard(cardButtonClicked.Card, false, out list);
                            if (dealtCardClicked != null)
                            {
                                // If the clicked card is a 2, 3, or 4, but is not on the required row, do not allow the move.
                                if (dealtCardClicked.PyramidRow < 3)
                                {
                                    var rankDifference = (cardAlreadySelected.Card.Rank - cardButtonClicked.Card.Rank);
                                    if ((rankDifference % 3 != 0))
                                    {
                                        await DisplayAlertAsync(
                                            MainPage.MyGetString("AccessibleSolitaire"),
                                            MainPage.MyGetString("RoyalparadeTargetCardOnWrongRow"),
                                            MainPage.MyGetString("OK"));

                                        cardButtonClicked.IsToggled = false;
                                        cardAlreadySelected.IsToggled = false;

                                        return;
                                    }
                                }

                                // Ok, we can now move ahead and move the card.

                                // First set the StackDetails for the two cards involved. The clicked card gets
                                // appended with tge rank of the already-clicked cards.

                                cardButtonClicked.StackDetails += " " + cardAlreadySelected.Card.Rank.ToString();

                                vm.DealtCards[dealtCardClicked.PyramidRow]
                                    [dealtCardClicked.PyramidCardOriginalIndexInRow].StackDetails =
                                        cardButtonClicked.StackDetails;

                                // Next set the card associated with the dealt cards, and the CardButtons.
                                // The card clicked adopts the playing card from the already-selected card.

                                vm.DealtCards[dealtCardClicked.PyramidRow]
                                    [dealtCardClicked.PyramidCardOriginalIndexInRow].Card =
                                        vm.DealtCards[dealtCardAlreadySelected.PyramidRow]
                                            [dealtCardAlreadySelected.PyramidCardOriginalIndexInRow].Card;

                                // The alread-selected card is either nulled at the playing card level or at 
                                // the dealt card array itself.
                                if (dealtCardAlreadySelected.PyramidRow < 3)
                                {
                                    vm.DealtCards[dealtCardAlreadySelected.PyramidRow]
                                        [dealtCardAlreadySelected.PyramidCardOriginalIndexInRow].Card = null;

                                    vm.DealtCards[dealtCardAlreadySelected.PyramidRow]
                                        [dealtCardAlreadySelected.PyramidCardOriginalIndexInRow].StackDetails = "";
                                }
                                else
                                {
                                    vm.DealtCards[dealtCardAlreadySelected.PyramidRow]
                                        [dealtCardAlreadySelected.PyramidCardOriginalIndexInRow] = null;
                                }

                                // Now update the associated CardButtons.
                                cardButtonClicked.Card = cardAlreadySelected.Card;

                                // Was the card that moved, moved from the fourth row?
                                if (dealtCardAlreadySelected.PyramidRow == 3)
                                {
                                    MoveCardFromFourthRow(dealtCardAlreadySelected, cardAlreadySelected);
                                }
                                else
                                {
                                    // A free spot has appeared in the first three rows, so clear its card 
                                    // but do not hide the spot.
                                    cardAlreadySelected.Card = null;
                                    cardAlreadySelected.IsVisible = true;

                                    cardAlreadySelected.StackDetails = "";
                                }

                                // The background colour of the card may change now.
                                cardButtonClicked.RefreshVisuals();
                            }
                        }
                        else
                        {
                            RoyalParadeMoveCardToEmptySlot(
                                cardButtonClicked,
                                cardAlreadySelected,
                                dealtCardAlreadySelected);
                        }
                    }

                    cardAlreadySelected.IsToggled = false;
                }

                if (cardButtonClicked != null)
                {
                    cardButtonClicked.IsToggled = false;
                }
            }
            else
            {
                // No card was already selected. Has an Ace been clicked?
                if ((cardButtonClicked != null) && (cardButtonClicked.Card != null))
                {
                    if (cardButtonClicked.Card.Rank == 1)
                    {
                        // Discard the ace.
                        DiscardAce(cardButtonClicked);

                        moveCard = true;
                    }
                    else
                    {
                        // No other card was selected, so we'll simply select the clicked card.
                        string? announcement = cardButtonClicked.CardPileAccessibleNameWithoutMofN;
                        MakeDelayedScreenReaderAnnouncement(announcement, false);
                    }
                }
            }

            if (cardAlreadySelected != null)
            {
                cardAlreadySelected.IsToggled = false;
            }

            PlaySound(moveCard);

            if (GameOver())
            {
                ShowEndOfGameDialog(false);
            }
        }

        private void MoveCardFromFourthRow(DealtCard dealtCardAlreadySelected, CardButton cardAlreadySelected)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // Find the index of the card beneath the card that moved from the fourth row.
            var cardRevealedIndex = dealtCardAlreadySelected.PyramidCardOriginalIndexInRow - 8;
            if (cardRevealedIndex < 0)
            {
                // Null out the slot for the dealt card.
                vm.DealtCards[3][dealtCardAlreadySelected.PyramidCardOriginalIndexInRow] = null;

                // This spot in the fourth row is now empty.
                SetRoyalParadeCardButtonEmpty(cardAlreadySelected);
            }
            else
            {
                // Show the revealed card.
                cardAlreadySelected.Card = vm.DealtCards[3][cardRevealedIndex].Card;

                if (cardAlreadySelected.Card == null)
                {
                    Debug.WriteLine("MoveCardFromFourthRow: Unexpected null card on revealed card on fourth row.");
                }

                cardAlreadySelected.StackDetails = vm.DealtCards[3][cardRevealedIndex].StackDetails;

                cardAlreadySelected.IsVisible = true;

                cardAlreadySelected.RefreshVisuals();
            }
        }

        private void DiscardAce(CardButton? cardButton)
        {
            if ((cardButton == null) || (cardButton.Card == null))
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
            if (dealtCard != null)
            {
                cardButton.IsToggled = false;

                dealtCard.Card = null;
                dealtCard.StackDetails = "";

                cardButton.StackDetails = "";

                if (dealtCard.PyramidRow < 3)
                {
                    cardButton.Card = null;
                }
                else
                {
                    MoveCardFromFourthRow(dealtCard, cardButton);
                }

                var discardMessage = MainPage.MyGetString("Discarded") + " " +
                                        dealtCard.AccessibleNameWithoutSelectionAndMofN + ". ";
                MakeDelayedScreenReaderAnnouncement(discardMessage, true);

                PlaySound(true);
            }
        }

        private async void RoyalparadePerformNextCardAction()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            if (_deckRemaining.Count <= 0)
            {
                return;
            }

            // There should always be a multiple of 8 cards left.

            Debug.WriteLine("RoyalparadePerformNextCardAction: Remaining card count " + _deckRemaining.Count);

            var announceDealMessage = true;

            for (int i = 0; i < 8; ++i)
            {
                var card = _deckRemaining[_deckRemaining.Count - 1];

                _deckRemaining.Remove(card);

                var currentPileCount = vm.DealtCards[i].Count;
                if (currentPileCount > 0)
                {
                    if ((currentPileCount > 1) || (vm.DealtCards[i][0].CardState != CardState.KingPlaceHolder))
                    {
                        // The pile is not empty, so mark the existing topmost card in the pile as no longer topmost.
                        var currentDealtCard = vm.DealtCards[i][currentPileCount - 1] as DealtCard;
                        if (currentDealtCard != null)
                        {
                            currentDealtCard.IsLastCardInPile = false;
                        }
                    }
                }

                // Now add the topmost card to the pile.
                var newDealtCard = new DealtCard();

                newDealtCard.FaceDown = false;
                newDealtCard.CardState = CardState.FaceUp;
                newDealtCard.Card = card;
                newDealtCard.Open = false;
                newDealtCard.IsLastCardInPile = true;

                // PyramidCardOriginalIndexInRow is the index of the bottom row pile containing the dealt card.
                newDealtCard.PyramidCardOriginalIndexInRow = i;

                // This card lies on the bottom row of the dealt card piles.
                newDealtCard.PyramidRow = 3;

                // Find the index of the next available slot for a card in this position on the fourth row.
                var index = i;

                var lowerCardStackDetails = "";

                while ((index < 72) && (vm.DealtCards[3][index] != null) && (vm.DealtCards[3][index].Card != null))
                {
                    lowerCardStackDetails = vm.DealtCards[3][index].StackDetails;

                    index += 8;
                }

                if (index >= 72)
                {
                    return;
                }

                newDealtCard.PyramidCardOriginalIndexInRow = index;
                newDealtCard.PyramidCardCurrentIndexInRow = index;

                //newDealtCard.PyramidCardCurrentCountOfCardsOnRow 

                vm.DealtCards[3][index] = newDealtCard;

                var cardButtonIndex = 24 + i;

                var cardButtonsUI = CardPileGridPyramid.Children;
                if (cardButtonsUI != null)
                {
                    var cardButtonsNext = cardButtonsUI[cardButtonIndex] as CardButton;
                    if (cardButtonsNext != null)
                    {
                        cardButtonsNext.Card = card;

                        var newStackDetails = lowerCardStackDetails + " " + card.Rank.ToString();
                        vm.DealtCards[3][index].StackDetails = newStackDetails;
                        cardButtonsNext.StackDetails = newStackDetails;

                        cardButtonsNext.IsVisible = true;
                    }
                }

                // Make sure its image is loaded to appear in the dealt card pile.
                TryToAddCardImageWithPictureToDictionary(newDealtCard);
            }

            // If we're not announcing a message about a sequence completing, announce the deal.
            if (announceDealMessage)
            {
                var announcement = MainPage.MyGetString("RoyalparadeDealtEightCards");
                MakeDelayedScreenReaderAnnouncement(announcement, false);
            }
        }
    }
}
