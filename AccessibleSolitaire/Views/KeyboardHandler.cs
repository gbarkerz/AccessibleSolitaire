using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;
using System.Diagnostics;

#if WINDOWS
using WindowsInput;
using WindowsInput.Native;
#endif 

namespace Sa11ytaire4All
{
    // This file contains code relating to reacting to keyboard input.
    public partial class MainPage : ContentPage
    {
        public void MoveToNearbyDealtCardPile(bool moveForward)
        {
            // Barker Note: I've not found a way of moving keyboard focus directly to an
            // item in a CollectionView. All I can do it move focus to the CollectionView
            // and then have the user press Tab to move to a contained item. That defeats
            // the object to reacting to an arrow press, so instead simulate a tab (or
            // shift tab) in response to the arrow press.

#if WINDOWS
            // The underlying functionality is only available on Windows.

            var inputSimulator = new InputSimulator();

            // Simulate pressing the Shift key if necessary.
            if (!moveForward)
            {
                inputSimulator.Keyboard.KeyDown(VirtualKeyCode.SHIFT);
            }

            // Simulate pressing the Tab key.
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.TAB);

            if (!moveForward)
            {
                inputSimulator.Keyboard.KeyUp(VirtualKeyCode.SHIFT);
            }
#endif
        }

        public void MoveBetweenPyramidRow(bool moveUp)
        {
            // Barker Todo: Surely there's a better way of doing this...
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                var cards = CardPileGridPyramid.Children;

                // Barker Todo: Understand why CardDeckUpturnedObscuredHigher and CardDeckUpturned
                // aren't found to be focused here.
                if (NextCardDeck.IsFocused)
                {
                    if (!moveUp && (cards.Count > 0))
                    {
                        var rowCard = cards[0] as CardButton;
                        if ((rowCard != null) && (rowCard.Card != null))
                        {
                            rowCard.Focus();
                        }
                    }
                }
                else
                {
                    foreach (var card in cards)
                    {
                        var rowCard = card as CardButton;
                        if ((rowCard != null) && (rowCard.Card != null))
                        {
                            if (IsCardButtonFocused(rowCard))
                            {
                                var targetRow = -1;

                                CollectionView? list;
                                var dealtCard = FindDealtCardFromCard(rowCard.Card, false, out list);
                                if (dealtCard != null)
                                {
                                    if (moveUp)
                                    {
                                        targetRow = dealtCard.PyramidRow - 1;
                                    }
                                    else
                                    {
                                        targetRow = dealtCard.PyramidRow + 1;
                                    }

                                    if ((targetRow >= 0) && (targetRow < 7))
                                    {
                                        CardButton? cardToFocus = null;

                                        foreach (var cardInPyramid in cards)
                                        {
                                            var rowCardInPyramid = cardInPyramid as CardButton;
                                            if ((rowCardInPyramid != null) && (rowCardInPyramid.Card != null))
                                            {
                                                var dealtCardToFocus = FindDealtCardFromCard(rowCardInPyramid.Card, false, out list);
                                                if ((dealtCardToFocus != null) && (dealtCardToFocus.PyramidRow == targetRow))
                                                {
                                                    cardToFocus = rowCardInPyramid;

                                                    break;
                                                }
                                            }
                                        }

                                        if (cardToFocus != null)
                                        {
                                            cardToFocus.Focus();
                                        }

                                        break;
                                    }
                                    else if (targetRow < 0)
                                    {
                                        NextCardDeck.Focus();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public string? AnnounceAvailableMoves(bool makeAnnouncement)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return null;
            }

            // Barker Note: We do not check whether a move from a target card pile is available down to 
            // a dealt card pile.

            // Barker Note: We don't check for moves to empty card piles, as that would fill the announcement.

            // Barker Note: When moving between dealt card piles, we only check whether the lowest face-up 
            // card in a pile can be moved, not any of the other face-up cards that might lie above that one.
            // This is important, because it's not the same dealt card as when checking whether a dealt card
            // can be moved to a Target cad pile. For a move from a dealt card pile to a Target card pile,
            // we only check the topmost card in the dealt card pile.

            int numberOfMoves = 0;
            string moveComment = "";
            DealtCard? destinationCard = null;
            DealtCard? sourceCard = null;

            // Barker Note: If this ever does get translated, consider using formatted strings here, 
            // rather than concatenating all the various strings.

            var moveAnnouncementInPileString = MyGetString("MoveAnnouncementInPile");
            var onString = MyGetString("On");
            var upturnedCardPile = MyGetString("UpturnedCardPile");
            var targetCardPile = MyGetString("TargetCardPile");
            var dealtCardPile = MyGetString("DealtCardPile");
            var canBeMovedTo = MyGetString("CanBeMovedTo");
            var noMoveIsAvailable = MyGetString("NoMoveIsAvailable");

            DealtCard? upturnedPseudoCard = null;
            if (_deckUpturned.Count > 0)
            {
                upturnedPseudoCard = new DealtCard();
                upturnedPseudoCard.FaceDown = false;
                upturnedPseudoCard.CardState = CardState.FaceUp;
                upturnedPseudoCard.Card = _deckUpturned[_deckUpturned.Count - 1];
            }

            // First check whether the upturned card can be moved to any of the target piles.
            var suitTarget = CanCardBeMovedToTargetPile(upturnedPseudoCard);
            if ((upturnedPseudoCard != null) && !string.IsNullOrEmpty(suitTarget))
            {
                ++numberOfMoves;

                // This is always the first string in the announcement.
                moveComment = upturnedPseudoCard.AccessibleNameWithoutSelectionAndMofN + " " +
                    onString + " " + upturnedCardPile + " " + canBeMovedTo + " " + suitTarget + " " + targetCardPile;
            }

            // First move through all the dealt card piles to determine if a card can be moved on to the
            // topmost card in a dealt card pile.

            // d is a destination for a move between dealt cards.
            for (int d = 0; d < cCardPiles; d++)
            {
                var destinationDealtCardPile = (CollectionView)CardPileGrid.FindByName("CardPile" + (d + 1));
                if (destinationDealtCardPile != null)
                {
                    var countOfCardsInPile = destinationDealtCardPile.ItemsSource.Cast<DealtCard>().Count();

                    var items = destinationDealtCardPile.ItemsSource;
                    foreach (var item in items)
                    {
                        destinationCard = item as DealtCard;
                        if (destinationCard != null)
                        {
                            // If this card is face down, ignore it.
                            if (destinationCard.CardState == CardState.FaceDown)
                            {
                                continue;
                            }

                            // Next check whether the upturned card pile can be moved on to the destination dealt card.
                            if (upturnedPseudoCard != null)
                            {
                                if (CanMoveCardToDealtCardPile(destinationCard, upturnedPseudoCard))
                                {
                                    if (numberOfMoves > 0)
                                    {
                                        moveComment += ", \r\n";
                                    }

                                    moveComment +=
                                        upturnedPseudoCard.AccessibleNameWithoutSelectionAndMofN + " " +
                                        onString + " " +
                                        upturnedCardPile + " " +
                                        canBeMovedTo + " " +
                                        destinationCard.AccessibleNameWithoutSelectionAndMofN + " " +
                                        moveAnnouncementInPileString + " " +
                                        dealtCardPile + " " + (d + 1).ToString();

                                    numberOfMoves++;
                                }
                            }

                            // Next check whether the destination card can be moved on to a target card pile.
                            // Only do this if this card has no other cards lying on top of it.
                            if (destinationCard.CurrentCardIndexInDealtCardPile == countOfCardsInPile - 1)
                            {
                                suitTarget = CanCardBeMovedToTargetPile(destinationCard);
                                if (!string.IsNullOrEmpty(suitTarget))
                                {
                                    if (numberOfMoves > 0)
                                    {
                                        moveComment += ", \r\n";
                                    }

                                    ++numberOfMoves;

                                    moveComment += destinationCard.AccessibleNameWithoutSelectionAndMofN +
                                        " " +
                                        onString + " " + dealtCardPile + " " + (d + 1).ToString() +
                                        " " + canBeMovedTo + " " +
                                        suitTarget + " " +
                                        MainPage.MyGetString("TargetCardPile");
                                }
                            }

                            // Now move through all the dealt card piles looking for a source card for amove.
                            for (int s = 0; s < cCardPiles; s++)
                            {
                                var sourceDealtCardPile = (CollectionView)CardPileGrid.FindByName("CardPile" + (s + 1));

                                // Don't attempt to move a card onto itself.
                                if ((destinationDealtCardPile != null) && (destinationDealtCardPile != sourceDealtCardPile))
                                {
                                    var itemsSource = vm.DealtCards[s];

                                    DealtCard sourceDealtCard = (itemsSource[0] as DealtCard);

                                    // If this source dealt card pile is either empty, or only holds a King, no move is possible.
                                    if (itemsSource.Count() == 1 &&
                                        ((sourceDealtCard.CardState == CardState.KingPlaceHolder) ||
                                        ((sourceDealtCard.Card != null) && (sourceDealtCard.Card.Rank == 13))))
                                    {
                                        continue;
                                    }

                                    // Find the lowest card in the dealt card pile that's face up because that's the one 
                                    // we want to see if it can be moved to the destinationCardPile.

                                    for (int cardsOnPile = itemsSource.Count; cardsOnPile > 0; --cardsOnPile)
                                    {
                                        sourceCard = itemsSource[cardsOnPile - 1] as DealtCard;
                                        if (sourceCard.CardState != CardState.FaceUp)
                                        {
                                            sourceCard = itemsSource[cardsOnPile] as DealtCard;

                                            break;
                                        }
                                    }

                                    // Ok, we now have the lowest face-up card in the source dealt card pile.
                                    if (sourceCard != null)
                                    {
                                        // Also, don't check whether an Ace in one dealt card pile can be moved onto 
                                        // a Two in another dealt card pile. That's not particularly helpful.
                                        if ((sourceCard.Card != null) && (sourceCard.Card.Rank == 1))
                                        {
                                            continue;
                                        }

                                        // Check whether the source card can be moved on top of another card in 
                                        // the destination pile.

                                        if (CanMoveCardToDealtCardPile(destinationCard, sourceCard))
                                        {
                                            if (numberOfMoves > 0)
                                            {
                                                moveComment += ", \r\n";
                                            }

                                            moveComment +=
                                                sourceCard.AccessibleNameWithoutSelectionAndMofN + " " +
                                                moveAnnouncementInPileString + " " +
                                                dealtCardPile + " " + localizedNumbers[s].ToString() + " " +
                                                canBeMovedTo + " " +
                                                destinationCard.AccessibleNameWithoutSelectionAndMofN + " " +
                                                moveAnnouncementInPileString + " " +
                                                dealtCardPile + " " + localizedNumbers[d].ToString();

                                            numberOfMoves++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (makeAnnouncement)
            {
                if (string.IsNullOrEmpty(moveComment))
                {
                    moveComment = noMoveIsAvailable;
                }

                Debug.WriteLine("Screen reader announce: " + moveComment);

                MakeDelayedScreenReaderAnnouncement(moveComment, false);
            }

            return moveComment;
        }

        private string CanCardBeMovedToTargetPile(DealtCard? card)
        {
            var suitTarget = "";

            List<Card>? targetPile = null;

            if ((card == null) || (card.Card == null))
            {
                return "";
            }

            switch (card.Card.Suit)
            {
                case Suit.Clubs:
                    targetPile = _targetPiles[0];
                    break;

                case Suit.Diamonds:
                    targetPile = _targetPiles[1];
                    break;

                case Suit.Hearts:
                    targetPile = _targetPiles[2];
                    break;

                case Suit.Spades:
                    targetPile = _targetPiles[3];
                    break;

                default:
                    break;
            }

            if (targetPile != null)
            {
                var canMoveCard = false;

                if (card.Card.Rank == 1)
                {
                    // An Ace can always be moved to the target pile.
                    canMoveCard = true;
                }
                else if (targetPile.Count > 0)
                {
                    // We know the suits match, so check whether the ranks are consecutive.
                    if (card.Card.Rank == targetPile[targetPile.Count - 1].Rank + 1)
                    {
                        canMoveCard = true;
                    }
                }

                if (canMoveCard)
                {
                    suitTarget = card.Card.GetSuitName();
                }
            }

            return suitTarget;
        }

        public void HandleF6(bool backwards)
        {
            if (NextCardDeck.IsFocused ||
                IsCardButtonFocused(CardDeckUpturnedObscuredLower) ||
                IsCardButtonFocused(CardDeckUpturnedObscuredHigher) ||
                IsCardButtonFocused(CardDeckUpturned))
            {
                if (!backwards)
                {
                    TargetPileC.Focus();
                }
                else
                {
                    MoveFocusToDealtCardPiles();
                }
            }
            else if (IsCardButtonFocused(TargetPileC) ||
                     IsCardButtonFocused(TargetPileD) ||
                     IsCardButtonFocused(TargetPileH) ||
                     IsCardButtonFocused(TargetPileS))
            {
                if (!backwards)
                {
                    MoveFocusToDealtCardPiles();
                }
                else
                {
                    NextCardDeck.Focus();
                }
            }
            else // Focus is on dealt card piles.
            {
                if (!backwards)
                {
                    NextCardDeck.Focus();
                }
                else
                {
                    TargetPileC.Focus();
                }
            }
        }

#if WINDOWS
        private Timer? timerDelayTabPress;
#endif

        private void MoveFocusToDealtCardPiles()
        {
            // Barker Note: I've not found a way of moving keyboard focus directly to an
            // item in a CollectionView. All I can do it move focus to the CollectionView
            // and then simulate a user press of Tab to move to a contained item.

            // Barker Note: If we had left keyboard focus on the dealt card piles Grid, it seems that
            // while keyboard focus moves to the Grid ok (or to CardPile1 if we chose to focus that
            // instead of the Grid), no UIA FocusChanged event is raised, and as such, a screen reader
            // doesn't announce anything.

            CardPileGrid.Focus();

#if WINDOWS
            if (timerDelayTabPress == null)
            {
                timerDelayTabPress = new Timer(
                    new TimerCallback((s) => DelayTabPress()),
                        null,
                        TimeSpan.FromMilliseconds(200),
                        TimeSpan.FromMilliseconds(Timeout.Infinite));
            }
#endif
        }

#if WINDOWS
        private void DelayTabPress()
        {
            timerDelayTabPress?.Dispose();
            timerDelayTabPress = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var inputSimulator = new InputSimulator();

                inputSimulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
            });
        }
#endif

        private bool IsCardButtonFocused(CardButton cardButton)
        {
            var isFocused = false;

            var innerButton = cardButton.FindByName("InnerButton") as Button;
            if (innerButton != null)
            {
                isFocused = innerButton.IsFocused;
            }

            return isFocused;
        }

        public async void ShowKeyboardShortcuts()
        {
            var popup = new ShortcutsPopup();

            await this.ShowPopupAsync(popup);
        }

        public void ToggleDealtCardSelectionFollowingKeyPress(DealtCard dealtCard)
        {
            if (dealtCard != null)
            {
                var collectionView = FindCollectionViewFromDealtCard(dealtCard);
                if (collectionView != null)
                {
                    if (dealtCard.CardSelected)
                    {
                        DeselectAllCardsFromDealtCardPile(collectionView);
                    }
                    else
                    {
                        collectionView.SelectedItem = dealtCard;
                    }
                }
            }
        }

        public void SelectDealtCardByPileNumber(int pileIndex)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm != null) && (vm.DealtCards != null))
            {
                var list = vm.DealtCards[pileIndex];
                if ((list != null) && (list.Count > 0))
                {
                    var dealtCard = vm.DealtCards[pileIndex][list.Count - 1];

                    var collectionView = (CollectionView)CardPileGrid.FindByName("CardPile" + (pileIndex + 1));

                    if (dealtCard.CardSelected)
                    {
                        TimedDelayDeselectDealtCard(collectionView);
                    }
                    else
                    {
                        TimedDelaySelectDealtCard(collectionView, dealtCard);
                    }
                }
            }
        }

        public void SelectUpturnedCard()
        {
            if (CardDeckUpturned.Card != null)
            {
                ClearAllSelections(false);

                CardDeckUpturned.IsToggled = !CardDeckUpturned.IsToggled;
            }
        }

        public void MoveSelectedCardToSuitPile()
        {
            var dealtCardSelected = false;
            var upturnedCardSelected = false;

            Suit selectedCardSuit = Suit.NoSuit;

            // Is a single dealt card selected?
            CollectionView? listAlreadySelected;
            int listAlreadySelectedIndex;
            var cardSelected = GetSelectedDealtCard(null, // List to be ignored.
                                             out listAlreadySelected,
                                             out listAlreadySelectedIndex);
            if (cardSelected != null)
            {
                if (cardSelected.Card != null)
                {
                    dealtCardSelected = true;

                    selectedCardSuit = cardSelected.Card.Suit;
                }
            }
            else
            {
                // Is the topmost upturned card selected?
                if (CardDeckUpturned.IsToggled)
                {
                    if (CardDeckUpturned.Card != null)
                    {
                        upturnedCardSelected = true;

                        selectedCardSuit = CardDeckUpturned.Card.Suit;
                    }
                }
            }

            if (dealtCardSelected || upturnedCardSelected)
            {
                CardButton? targetCardButton = null;

                switch (selectedCardSuit)
                {
                    case Suit.Clubs:
                        targetCardButton = TargetPileC;
                        break;

                    case Suit.Diamonds:
                        targetCardButton = TargetPileD;
                        break;

                    case Suit.Hearts:
                        targetCardButton = TargetPileH;
                        break;

                    case Suit.Spades:
                        targetCardButton = TargetPileS;
                        break;
                }

                if (targetCardButton != null)
                {
                    if (dealtCardSelected)
                    {
                        // A check for the game being complete is made beneath this call.
                        MoveDealtCardToTargetPileAsAppropriate(targetCardButton);
                    }
                    else
                    {
                        var cardWasMoved = MoveUpturnedCardToTargetPileAsAppropriate(targetCardButton);
                        if (cardWasMoved)
                        {
                            // A card was moved from the Upturned Card Pile to a Target Card Pile.
                            if (GameOver())
                            {
                                ShowEndOfGameDialog();
                            }
                        }
                    }
                }
            }
        }
    }
}