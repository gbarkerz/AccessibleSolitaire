using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;
using System.Diagnostics;

namespace Sa11ytaire4All
{
    public sealed partial class MainPage : ContentPage
    {
        private void DeselectCard(DealtCard dealtCard)
        {
            dealtCard.CardSelected = false;
            dealtCard.RefreshVisuals();

            // All cards above this deselected card in a dealt card pile are no longer
            // is the set of cards that will move along with the selected card.
            SetInSelectedSet(dealtCard);
        }

        private void DeselectFirstSelectedCardInCollectionView(CollectionView collectionView)
        {
            var items = collectionView.ItemsSource;
            foreach (var item in items)
            {
                var dealtCard = item as DealtCard;
                if (dealtCard != null)
                {
                    if (dealtCard.CardSelected)
                    {
                        DeselectCard(dealtCard);

                        break;
                    }
                }
            }
        }

        // The selection state of one of the card in the Dealt Card piles has changed.
        private void CardPile_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsCurrentGamePaused())
            {
                ShowGameIsPausedMessage();

                return;
            }

            var listSelectionChanged = sender as CollectionView;
            if (listSelectionChanged == null)
            {
                return;
            }

            try
            {
                // Only take action when a card has been selected.
                if ((e.CurrentSelection == null) || (e.CurrentSelection.Count == 0))
                {
                    //Debug.WriteLine("CardPile_SelectionChanged: No selection in " + listSelectionChanged.AutomationId);

                    // A card is being deselected, so we must make sure no selection feedback is
                    // left showing on the cards. At most only one card can currently be selected.
                    DeselectFirstSelectedCardInCollectionView(listSelectionChanged);

                    return;
                }

                // If the selected card is face down, unselect it and do nothing more.
                var selectedCard = e.CurrentSelection[0] as DealtCard;
                if (selectedCard == null)
                {
                    return;
                }

                if ((currentGameType == SolitaireGameType.Bakersdozen) && !selectedCard.IsLastCardInPile)
                {
                    return;
                }

                if (selectedCard.FaceDown)
                {
                    MakeDelayedScreenReaderAnnouncement(
                        MainPage.MyGetString("FaceDownCardsCannotBeSelected"), false);

                    if (listSelectionChanged != null)
                    {
                        DeselectAllCardsFromDealtCardPile(listSelectionChanged);
                    }

                    DeselectCard(selectedCard);

                    return;
                }

                // TESTING: Check that the card being selected is the one most recently tapped
                //if (DateTime.Now - timePreviousDealtCardTap < TimeSpan.FromMilliseconds(2000))
                //{
                //    DealtCard? selectedDealtCard = listSelectionChanged.SelectedItem as DealtCard;
                //    if ((selectedDealtCard != null) && ((selectedDealtCard.Card != null)))
                //    {
                //        if ((mostRecentlyTappedCard.Rank != selectedDealtCard.Card.Rank) ||
                //            (mostRecentlyTappedCard.Suit != selectedDealtCard.Card.Suit))
                //        {
                //            var message = "I'm sorry, but there's a problem with the app. The " +
                //                            mostRecentlyTappedCard.Rank.ToString() + " of " +
                //                            mostRecentlyTappedCard.Suit.ToString() + 
                //                            " was tapped, but the selection then changed for the " +
                //                            selectedDealtCard.Card.Rank.ToString() + " of " +
                //                            selectedDealtCard.Card.Suit.ToString() + ".";

                //            Debug.WriteLine(message);

                //            await DisplayAlertAsync(
                //                MainPage.MyGetString("AccessibleSolitaire"),
                //                message,
                //                MainPage.MyGetString("OK"));

                //            //SentrySdk.CaptureMessage(message, SentryLevel.Debug);
                //        }
                //    }
                //}

                //Debug.WriteLine("CardPile_SelectionChanged: Selection of " + selectedCard.AccessibleNameWithoutSelectionAndMofN);

                // Has an empty card pile been selected?
                if (selectedCard.CardState == CardState.KingPlaceHolder)
                {
                    // In Baker's Dozen, a card cannot be moved to an empty pile.
                    if (currentGameType != SolitaireGameType.Bakersdozen)
                    {
                        // Attempt to move the upturned card over to the empty card pile.
                        if (timerDelayAttemptToMoveCard == null)
                        {
                            timerDelayAttemptToMoveCard = new Timer(
                                new TimerCallback((s) => TimedDelayAttemptToMoveCardToEmptyPile(listSelectionChanged)),
                                    null,
                                    TimeSpan.FromMilliseconds(500),
                                    TimeSpan.FromMilliseconds(Timeout.Infinite));
                        }

                        DeselectAllCardsFromDealtCardPile(listSelectionChanged);
                    }
                    else
                    {
                        PlaySound(false);

                        var announcement = MainPage.MyGetString("NoMoveToEmptyPile");
                        MakeDelayedScreenReaderAnnouncement(announcement, false);
                    }

                    // Never leave the empty slot selected.
                    selectedCard.CardSelected = false;
                }
                else if (CardDeckUpturned.IsToggled && (_deckUpturned.Count > 0))
                {
                    // Attempt to move the upturned card to this list.
                    DealtCard? cardUpturned = new DealtCard();
                    cardUpturned.CardState = CardState.FaceUp;
                    cardUpturned.Card = _deckUpturned[_deckUpturned.Count - 1];

                    // cardDealtPile is the selected card in the CardPile list.
                    DealtCard? cardDealtPile = listSelectionChanged.SelectedItem as DealtCard;
                    if (cardDealtPile == null)
                    {
                        return;
                    }

                    bool movedCard = false;

                    if (CanMoveCardToDealtCardPile(cardDealtPile, cardUpturned))
                    {
                        // Always deselect the selected item prior to moving anything between lists.
                        DeselectCard(cardDealtPile);
                        DeselectCard(cardUpturned);

                        // On Android, deselecting a card inside the tap handler seems not to work.
                        // So delay the deselection a little until we're out of the tap handler.

                        timerDeselectDealtCard = new Timer(
                            new TimerCallback((s) => TimedDelayDeselectDealtCard(listSelectionChanged)),
                                null,
                                TimeSpan.FromMilliseconds(500),
                                TimeSpan.FromMilliseconds(Timeout.Infinite));

                        // Move the upturned card to the CardPile list.
                        var itemsAdded = GetListSource(listSelectionChanged);
                        if (itemsAdded == null)
                        {
                            return;
                        }

                        itemsAdded.Add(cardUpturned);

                        cardDealtPile.IsLastCardInPile = false;
                        cardUpturned.IsLastCardInPile = true;

                        var listSelectedIndex = GetDealtPileIndexFromCollectionView(listSelectionChanged);
                        cardUpturned.CurrentDealtCardPileIndex = listSelectedIndex;
                        cardUpturned.CurrentCardIndexInDealtCardPile = itemsAdded.Count - 1;

                        RemoveDealtCardFromDealtCardList(_deckUpturned, cardUpturned);

                        SetUpturnedCardsVisuals();

                        string announcement =
                            MainPage.MyGetString("Moved") + " " +
                            cardUpturned.Card.GetCardAccessibleName() + " " +
                            MainPage.MyGetString("To") + " " +
                            MainPage.MyGetString("DealtCardPile") + " " +
                            localizedNumbers[listSelectedIndex] +
                            ".";

                        MakeDelayedScreenReaderAnnouncement(announcement, true);

                        RefreshDealtCardPileAccessibleNames(listSelectionChanged);

                        // Make sure the card moved to the dealt card pile shows the correct suit colours.
                        cardUpturned.RefreshVisuals();

                        movedCard = true;
                    }
                    else
                    {
                        // The upturned card cannot be moved to the newly-selected dealt card.
                        if (cardUpturned.Card != null)
                        {
                            AnnounceNoMove(cardUpturned.Card);
                        }
                    }

                    ClearAllSelections(true);

                    PlaySound(movedCard);
                }
                else
                {
                    // Attempt to move a card from a target pile to a dealt card pile.
                    if (!MoveTargetPileCardToDealtCardPileAsAppropriate(listSelectionChanged))
                    {
                        // Attempt to move a card between dealt card piles.
                        MoveCardBetweenDealtCardPiles(listSelectionChanged);
                    }

                    ClearCardButtonSelections(true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CardPile_SelectionChanged: " + ex.Message);
            }
        }

        private int GetDealtPileIndexFromCollectionView(CollectionView dealtPileCollectionView)
        {
            var dealtPileCollectionViewIndex = int.Parse(dealtPileCollectionView.AutomationId.Replace("CardPile", ""));

            // The dealt piles Names and AutomationIds are one-based, but we need the zero-based index here.
            return dealtPileCollectionViewIndex - 1;
        }

        private void TimedDelayAttemptToMoveCardToEmptyPile(object list)
        {
            timerDelayAttemptToMoveCard?.Dispose();
            timerDelayAttemptToMoveCard = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var listSelectionChanged = list as CollectionView;
                if (listSelectionChanged != null)
                {
                    DelayAttemptToMoveCardToEmptyPile(listSelectionChanged);
                }
            });
        }

        private void DelayAttemptToMoveCardToEmptyPile(CollectionView? listSelected)
        {
            if (listSelected == null)
            { 
                return; 
            }

            // cardAbove here will be the card being moved to the empty dealt card pile.
            DealtCard? cardMoving = null;

            bool movedCard = false;

            // Consider moving the upturned card to this Card Pile.
            if (CardDeckUpturned.IsToggled && (_deckUpturned.Count > 0))
            {
                // Is the upturned card a King?
                var movingCard = _deckUpturned[_deckUpturned.Count - 1];

                if (CanCardBeMovedToEmptyDealtCardPile(movingCard))
                {
                    // Remove the empty card from the dealt card pile.
                    var itemsAdded = GetListSource(listSelected);
                    if (itemsAdded == null)
                    {
                        return;    
                    }

                    itemsAdded.RemoveAt(0);

                    // Add a new card to add to the dealt card pile.
                    cardMoving = new DealtCard();
                    cardMoving.CardState = CardState.FaceUp;
                    cardMoving.Card = movingCard;

                    DealtCard cardAddedToPile = new DealtCard();
                    cardAddedToPile.CardState = CardState.FaceUp;
                    cardAddedToPile.Card = cardMoving.Card;
                    cardAddedToPile.IsLastCardInPile = true;

                    itemsAdded.Add(cardAddedToPile);

                    var listSelectedIndex = GetDealtPileIndexFromCollectionView(listSelected);
                    cardAddedToPile.CurrentDealtCardPileIndex = listSelectedIndex;
                    cardAddedToPile.CurrentCardIndexInDealtCardPile = itemsAdded.Count - 1;

                    // Now remove the card from the upturned card pile.
                    RemoveDealtCardFromDealtCardList(_deckUpturned, cardMoving);

                    SetUpturnedCardsVisuals();

                    string announcement =
                        MainPage.MyGetString("Moved") + " " +
                        cardAddedToPile.Card.GetCardAccessibleName() + " " +
                        MainPage.MyGetString("To") + " " +
                        MainPage.MyGetString("DealtCardPile") + " " +
                        localizedNumbers[listSelectedIndex] +
                        ".";

                    MakeDelayedScreenReaderAnnouncement(announcement, true);

                    ClearCardButtonSelections(true);

                    movedCard = true;
                }
            }
            else
            {
                // Get the already-selected card from the other list if there is one.
                CollectionView? listAlreadySelected;
                int listAlreadySelectedIndex;
                cardMoving = GetSelectedDealtCard(listSelected, // List to be ignored.
                                                 out listAlreadySelected,
                                                 out listAlreadySelectedIndex);
                if (listAlreadySelected == null)
                {
                    return;
                }

                if (cardMoving != null)
                {
                    // Always deselect the selected item prior to moving anything between lists.
                    DeselectCard(cardMoving);

                    DeselectAllCardsFromDealtCardPile(listSelected);

                    // Is the already selected card a King?
                    if ((cardMoving.Card != null) && CanCardBeMovedToEmptyDealtCardPile(cardMoving.Card))
                    {
                        MoveKingToEmptyCard(cardMoving, listSelected, listAlreadySelected, listAlreadySelectedIndex);

                        movedCard = true;
                    }
                }
            }

            ClearCardButtonSelections(true);

            PlaySound(movedCard);
        }

        private bool CanCardBeMovedToEmptyDealtCardPile(Card card)
        {
            // If only Kings can be moved to an empty card pile, make sure this is a King.
            return (!OptionKingsOnlyToEmptyPile || (card.Rank == 13));
        }

        private void MoveKingToEmptyCard(DealtCard cardKing, CollectionView listEmpty, CollectionView listKing, int listKingIndex)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            var checkForAutoComplete = false;

            // Always deselect the selected items prior to moving anything between lists.
            DeselectAllCardsFromDealtCardPile(listEmpty);
            DeselectAllCardsFromDealtCardPile(listKing);

            var listArray = vm.DealtCards[listKingIndex];

            // A King is selected in the other Card Pile list.
            int movingCardIndex = listArray.IndexOf(cardKing);

            DealtCard? cardRevealed = null;

            var itemsAdded = GetListSource(listEmpty);
            var itemsRemoved = GetListSource(listKing);

            if ((itemsAdded == null) || (itemsRemoved == null))
            {
                return;
            }

            // Is the king being moved the first card in that list?
            if (movingCardIndex > 0)
            {
                // No, so show the card that was previously beneath the moving King.
                cardRevealed = (DealtCard)listArray[movingCardIndex - 1];
            }

            // Remove the empty card from the dealt card pile.
            itemsAdded.RemoveAt(0);

            var listSelectedIndex = GetDealtPileIndexFromCollectionView(listEmpty);

            var listArrayCount = listArray.Count;
            var itemsAddedCount = itemsAdded.Count;

            // Now move all cards from the source pile to the empty dealt card pile.
            while (movingCardIndex < listArrayCount)
            {
                --listArrayCount;

                var nextCard = (DealtCard)listArray[movingCardIndex];

                itemsRemoved.Remove(nextCard);

                nextCard.CurrentDealtCardPileIndex = listSelectedIndex;
                nextCard.CurrentCardIndexInDealtCardPile = itemsAddedCount;

                itemsAdded.Add(nextCard);

                ++itemsAddedCount;
            }

            var listEmptyIndex = GetDealtPileIndexFromCollectionView(listEmpty);

            var cardRevealedName = "";

            if (cardRevealed != null)
            {
                // Update the count of face-down cards in the pile in the bottom card in the pile.
                UpdatePileFaceDownCount(listArray, cardRevealed);

                // The card being revealed is no longer face-down, and is now the
                // last card in the list.
                cardRevealed.FaceDown = false;
                cardRevealed.CardState = CardState.FaceUp;
                cardRevealed.IsLastCardInPile = true;

                cardRevealedName = cardRevealed.AccessibleNameWithoutSelectionAndMofN;

                checkForAutoComplete = true;

                cardRevealed.RefreshVisuals();
            }
            else
            {
                // Add an "empty card" item to the list.
                AddEmptyCardToCollectionView(itemsRemoved, listEmptyIndex);

                cardRevealedName = MainPage.MyGetString("EmptyCardPile");
            }

            string inDealtCardPile = MainPage.MyGetString("InDealtCardPile");
            string revealedString = MainPage.MyGetString("Revealed");

            if (cardKing.Card == null)
            {
                return;
            }

            string announcement =
                MainPage.MyGetString("Moved") + " " +
                cardKing.Card.GetCardAccessibleName() + " " +
                MainPage.MyGetString("To") + " " +
                MainPage.MyGetString("Empty") + " " +
                MainPage.MyGetString("DealtCardPile") + " " +
                localizedNumbers[listEmptyIndex];
                
            if (currentGameType == SolitaireGameType.Klondike)
            {
                announcement += ", " +
                    revealedString + " " +
                    cardRevealedName + " " +
                    inDealtCardPile + " " +
                    localizedNumbers[listKingIndex];
            }

            announcement += ".";

            MakeDelayedScreenReaderAnnouncement(announcement, true);

            RefreshDealtCardPileAccessibleNames(listEmpty);
            RefreshDealtCardPileAccessibleNames(listKing);

            if (checkForAutoComplete && CheckForAutoComplete())
            {
                AutoCompleteGameNow(true);
            }
        }

        private bool MoveTargetPileCardToDealtCardPileAsAppropriate(CollectionView listDealtCardPile)
        {
            bool movedCard = false;

            CardButton? cardButton = null;
            List<Card>? listTargetPile = null;

            if (TargetPileC.IsToggled)
            {
                cardButton = TargetPileC;
                listTargetPile = _targetPiles[0];
            }
            else if (TargetPileD.IsToggled)
            {
                cardButton = TargetPileD;
                listTargetPile = _targetPiles[1];
            }
            else if (TargetPileH.IsToggled)
            {
                cardButton = TargetPileH;
                listTargetPile = _targetPiles[2];
            }
            else if (TargetPileS.IsToggled)
            {
                cardButton = TargetPileS;
                listTargetPile = _targetPiles[3];
            }

            if ((cardButton != null) && (listTargetPile != null) && (listTargetPile.Count > 0))
            {
                try
                {
                    if (listDealtCardPile.SelectedItem != null)
                    {
                        DealtCard? cardBelow = listDealtCardPile.SelectedItem as DealtCard;
                        if (cardBelow == null)
                        {
                            return false;
                        }

                        var cardAbove = new DealtCard();
                        cardAbove.CardState = CardState.FaceUp;

                        cardAbove.Card = listTargetPile[listTargetPile.Count - 1];
                        cardAbove.IsLastCardInPile = false;

                        if (CanMoveCardToDealtCardPile(cardBelow, cardAbove))
                        {
                            // Move the card from the TargetPile to this CardPile list.
                            RemoveDealtCardFromDealtCardList(listTargetPile, cardAbove);

                            if (listTargetPile.Count == 0)
                            {
                                cardButton.Card = null;
                            }
                            else
                            {
                                cardButton.Card = listTargetPile[listTargetPile.Count - 1];
                            }

                            var itemsAdded = GetListSource(listDealtCardPile);
                            if (itemsAdded == null)
                            {
                                return false;
                            }

                            var itemsAddedCurrentCount = itemsAdded.Count;

                            // What was previously the last card in the destination pile, is not longer so.
                            itemsAdded[itemsAddedCurrentCount - 1].IsLastCardInPile = false;

                            // Now add the moved card to the destination pile.
                            itemsAdded.Add(cardAbove);

                            ++itemsAddedCurrentCount;

                            cardAbove.IsLastCardInPile = true;

                            var listSelectedIndex = GetDealtPileIndexFromCollectionView(listDealtCardPile);
                            cardAbove.CurrentDealtCardPileIndex = listSelectedIndex;
                            cardAbove.CurrentCardIndexInDealtCardPile = itemsAddedCurrentCount - 1;

                            movedCard = true;

                            var announcedDealtCardIndex = 0;

                            // Get the index of the list containing the selected cards.
                            for (int i = 0; i < GetCardPileCount(); i++)
                            {
                                var list = (CollectionView)CardPileGrid.FindByName("CardPile" + (i + 1));
                                if (list == listDealtCardPile)
                                {
                                    announcedDealtCardIndex = i;

                                    break;
                                }
                            }

                            string announcement =
                                MainPage.MyGetString("Moved") + " " +
                                cardAbove.Card.GetCardAccessibleName() + " " +
                                MainPage.MyGetString("To") + " " +
                                MainPage.MyGetString("DealtCardPile") + " " +
                                localizedNumbers[announcedDealtCardIndex] +
                                ".";

                            MakeDelayedScreenReaderAnnouncement(announcement, true);

                            RefreshDealtCardPileAccessibleNames(listDealtCardPile);
                        }
                        else
                        {
                            // The card from the target card pile cannot be moved to the already-selected dealt card.
                            if (cardAbove.Card != null)
                            {
                                AnnounceNoMove(cardAbove.Card);
                            }
                        }

                        DeselectCard(cardBelow);
                        DeselectCard(cardAbove);
                        
                        PlaySound(movedCard);
                    }

                    DeselectAllCardsFromDealtCardPile(listDealtCardPile);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MoveTargetPileCardToDealtCardPileAsAppropriate: " + ex.ToString());
                }
            }

            return movedCard;
        }

        private bool MoveCardBetweenDealtCardPiles(CollectionView? listSelectionChanged)
        {
            if ((listSelectionChanged == null) || (listSelectionChanged.SelectedItem == null))
            {
                return false;
            }

            bool movedCard = false;

            // Get the card that has just been selected. The idea being already-selected card
            // will be placed above the card that's just been selected.
            var cardBelow = listSelectionChanged.SelectedItem as DealtCard;
            if (cardBelow == null)
            {
                return false;
            }

            // Get the already-selected card from the other list if there is one.
            CollectionView? listAlreadySelected;
            int listAlreadySelectedIndex;
            var cardAbove = GetSelectedDealtCard(listSelectionChanged,
                                                 out listAlreadySelected,
                                                 out listAlreadySelectedIndex);
            if (listAlreadySelected == null)
            {
                // No other dealt card was already selected. All other checks for action with this card
                // have been executed by now, so all that is happening here is that a dealt card is being 
                // selected. As such, announce this result. Note that the new selection state is already
                // incorporated into the card name.
                cardBelow.CardSelected = true;
                cardBelow.RefreshVisuals();

                // All cards above this selected card in the dealt card pile will move along with the 
                // selected card, so update their visuals to make sure this is clear to the player.
                SetInSelectedSet(cardBelow);

                string? announcement = cardBelow.AccessibleName;

                MakeDelayedScreenReaderAnnouncement(announcement, false);

                return false; 
            }
            else if (listAlreadySelected == listSelectionChanged)
            {
                if (cardAbove != null)
                {
                    DeselectCard(cardAbove);
                }

                cardBelow.CardSelected = true;
                SetInSelectedSet(cardBelow);
            }

            if (cardAbove != null)
            {
                // Always deselect the selected items prior to moving anything between lists.
                DeselectCard(cardBelow);
                DeselectCard(cardAbove);

                DeselectAllCardsFromDealtCardPile(listAlreadySelected);

                // On Android, deselecting a card inside the tap handler seems not to work.
                // So delay the deselection a little until we're out of the tap handler.

                timerDeselectDealtCard = new Timer(
                    new TimerCallback((s) => TimedDelayDeselectDealtCard(listSelectionChanged)),
                        null,
                        TimeSpan.FromMilliseconds(500),
                        TimeSpan.FromMilliseconds(Timeout.Infinite));

                var movingCardData = new MovingCardData{
                    CardBelow = cardBelow,
                    CardAbove = cardAbove,
                    ListAlreadySelected = listAlreadySelected,
                    ListAlreadySelectedIndex = listAlreadySelectedIndex, 
                    ListSelectionChanged = listSelectionChanged};

                if (timerDelayAttemptToMoveCard == null)
                {
                    timerDelayAttemptToMoveCard = new Timer(
                        new TimerCallback((s) => TimedDelayAttemptToMoveCardBetweenPiles(movingCardData)),
                            null,
                            TimeSpan.FromMilliseconds(500),
                            TimeSpan.FromMilliseconds(Timeout.Infinite));
                }
            }
            else
            {
                // No card has been moved, so simply announce the selection.
                var announcedSelectedIndex = 0;

                // Get the index of the list containing the selected cards.
                for (int i = 0; i < GetCardPileCount(); i++)
                {
                    var list = (CollectionView)CardPileGrid.FindByName("CardPile" + (i + 1));
                    if (list == listSelectionChanged)
                    {
                        announcedSelectedIndex = i;

                        break;
                    }
                }

                string inDealtCardPile = MainPage.MyGetString("InDealtCardPile");

                string announcement =
                    cardBelow.AccessibleName + " " + inDealtCardPile + " " +
                    localizedNumbers[announcedSelectedIndex] + ".";

                MakeDelayedScreenReaderAnnouncement(announcement, false);

                // Be sure to leave the selected DealtCard selected.
                cardBelow.CardSelected = true;
                SetInSelectedSet(cardBelow);

            }

            return movedCard;
        }

        private void TimedDelayAttemptToMoveCardBetweenPiles(MovingCardData movingCardData)
        {
            timerDelayAttemptToMoveCard?.Dispose();
            timerDelayAttemptToMoveCard = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DelayedAttemptToMoveCardBetweenPiles(movingCardData);
            });
        }

        private void TimedDelayMoveDealtCardToTargetPileAsAppropriate(MovingCardData movingCardData)
        {
            timerDelayAttemptToMoveCard?.Dispose();
            timerDelayAttemptToMoveCard = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DelayMoveDealtCardToTargetPileAsAppropriate(movingCardData);
            });
        }

        // IMPORTANT:
        // Any attempt to move a dealt card to another dealt card pile or a target card pile
        // must only happen after iOS has fully reacted to a change in the card's visuals 
        // after its selection state changes. Otherwise the app crashes on iOS. So if a 
        // dealt card might be removed from a dealt card pile, always delay this action 
        // a little in order to give the OS a chance to complete any selection state change.

        private class MovingCardData
        {
            public DealtCard? CardBelow;
            public DealtCard? CardAbove;
            public CollectionView? ListAlreadySelected;
            public int ListAlreadySelectedIndex;
            public CollectionView? ListSelectionChanged;
            public CardButton? TargetPileCardButton;
            public int TargetPileIndex;
        }

        private void DelayedAttemptToMoveCardBetweenPiles(MovingCardData movingCardData)
        {
            var checkForAutoComplete = false;

            DealtCard? cardBelow = movingCardData.CardBelow;
            DealtCard? cardAbove = movingCardData.CardAbove;

            if ((cardBelow == null) || (cardAbove == null))
            {
                return;
            }

            var listAlreadySelected = movingCardData.ListAlreadySelected;
            var listAlreadySelectedIndex = movingCardData.ListAlreadySelectedIndex;
            var listSelectionChanged = movingCardData.ListSelectionChanged;

            if ((listSelectionChanged == null) || (listAlreadySelected == null))
            {
                return;
            }

            bool movedCard = false;

            // Is a valid move possible between card piles?
            if (CanMoveCardToDealtCardPile(cardBelow, cardAbove))
            {
                var vm = this.BindingContext as DealtCardViewModel;
                if ((vm == null) || (vm.DealtCards == null))
                {
                    return;
                }

                // Move the card (or cards) from the other list to this CardPile list.
                // Typically a card that was face-down in the other list will be turned
                // up to reveal a card.
                DealtCard? cardRevealed = null;

                var sourceCardArray = vm.DealtCards[listAlreadySelectedIndex];

                // Get the index of the card being moved from the other list.
                var movingCardIndex = sourceCardArray.IndexOf(cardAbove);
                if (movingCardIndex > 0)
                {
                    // We will reveal the face-down card in the list where the card is moving from.
                    cardRevealed = (DealtCard)sourceCardArray[movingCardIndex - 1];
                }

                // Get the card lists for the list where the card is to be removed,
                // and the list where the card is to be added.
                var itemsRemoved = GetListSource(listAlreadySelected);
                var itemsAdded = GetListSource(listSelectionChanged);

                if ((itemsAdded == null) || (itemsRemoved == null))
                {
                    return;
                }

                // Move multiple cards from the source list if necessary. Note that we don't
                // have to change the IsLastCardInPile values for the cards being moved.
                var index = GetDealtPileIndexFromCollectionView(listAlreadySelected);
                var sourceArray = vm.DealtCards[index];

                var listSelectedIndex = GetDealtPileIndexFromCollectionView(listSelectionChanged);

                var sourceArrayCount = sourceArray.Count;
                var itemsAddedCurrentCount = itemsAdded.Count;

                while (movingCardIndex < sourceArrayCount)
                {
                    --sourceArrayCount;

                    var nextCard = (DealtCard)sourceArray[movingCardIndex];

                    itemsRemoved.Remove(nextCard);

                    // Update the stored pile index and card index for the card being moved.
                    nextCard.CurrentCardIndexInDealtCardPile = itemsAddedCurrentCount;
                    nextCard.CurrentDealtCardPileIndex = listSelectedIndex;

                    // No cards are selected after the move.
                    DeselectCard(nextCard);
                    nextCard.InSelectedSet = false;

                    itemsAdded.Add(nextCard);

                    ++itemsAddedCurrentCount;
                }

                var cardRevealedName = "";

                // Was the card being moved the only item in the source list?
                if (cardRevealed != null)
                {
                    // Update the count of face-down cards in the pile in the bottom card in the pile.
                    UpdatePileFaceDownCount(sourceArray, cardRevealed);

                    // The card being revealed is no longer face-down, and is now the
                    // last card in the list.
                    cardRevealed.FaceDown = false;
                    cardRevealed.CardState = CardState.FaceUp;
                    cardRevealed.IsLastCardInPile = true;

                    cardRevealedName = cardRevealed.AccessibleNameWithoutSelectionAndMofN;

                    checkForAutoComplete = true;

                    cardRevealed.RefreshVisuals();
                }
                else
                {
                    // Add an "empty card" item to the list.
                    AddEmptyCardToCollectionView(itemsRemoved, listAlreadySelectedIndex);

                    cardRevealedName = MainPage.MyGetString("EmptyCardPile");
                }

                string inDealtCardPile = MainPage.MyGetString("InDealtCardPile");
                string revealedString = MainPage.MyGetString("Revealed");

                if (cardAbove.Card != null)
                {
                    string announcement =
                        MainPage.MyGetString("Moved") + " " +
                        cardAbove.Card.GetCardAccessibleName();

                    if (currentGameType == SolitaireGameType.Klondike)
                    {
                        announcement += ", " +
                            revealedString + " " +
                            cardRevealedName + " " +
                            inDealtCardPile + " " +
                            localizedNumbers[listAlreadySelectedIndex];
                    }

                    announcement += ".";

                    MakeDelayedScreenReaderAnnouncement(announcement, true);
                }

                cardBelow.IsLastCardInPile = false;

                movedCard = true;
            }
            else
            {
                // The newly-selected dealt card cannot be moved to the already-selected dealt card.
                if (cardAbove.Card != null)
                {
                    AnnounceNoMove(cardAbove.Card);
                }
            }

            RefreshDealtCardPileAccessibleNames(listAlreadySelected);
            RefreshDealtCardPileAccessibleNames(listSelectionChanged);

            // Another card was already selected. Play a sound to refect whether a move occurred.
            PlaySound(movedCard);

            if (checkForAutoComplete && CheckForAutoComplete())
            {
                AutoCompleteGameNow(true);
            }
        }

        private void AnnounceNoMove(Card card)
        {
            if (card != null)
            {
                string announcement =
                    MainPage.MyGetString("NoMove") + ". " +
                    MainPage.MyGetString("Deselected") + " " +
                    card.GetCardAccessibleName() + ".";

                MakeDelayedScreenReaderAnnouncement(announcement, false);
            }
        }

        private void DelayMoveDealtCardToTargetPileAsAppropriate(MovingCardData movingCardData)
        {
            var cardAbove = movingCardData.CardAbove;
            var listAlreadySelected = movingCardData.ListAlreadySelected;
            var listAlreadySelectedIndex = movingCardData.ListAlreadySelectedIndex;
            var targetPileCardButton = movingCardData.TargetPileCardButton;
            var targetPileIndex = movingCardData.TargetPileIndex;

            if ((targetPileCardButton == null) || (listAlreadySelected == null))
            {
                return;
            }

            // Only the last card in a dealt card pile can be moved up to a target card pile.
            if ((cardAbove != null) && (cardAbove.IsLastCardInPile))
            {
                // Ok, we've found a selected item in one of the Dealt Card lists.
                // cardAbove here is the selected card in the dealt card pile.

                string inDealtCardPile = MainPage.MyGetString("InDealtCardPile");
                string revealedString = MainPage.MyGetString("Revealed");

                if (cardAbove.Card == null)
                {
                    return;
                }

                // No action if the select card's suit does not match the Target Pile.
                if (((targetPileIndex == 0) && (cardAbove.Card.Suit != Suit.Clubs)) ||
                    ((targetPileIndex == 1) && (cardAbove.Card.Suit != Suit.Diamonds)) ||
                    ((targetPileIndex == 2) && (cardAbove.Card.Suit != Suit.Hearts)) ||
                    ((targetPileIndex == 3) && (cardAbove.Card.Suit != Suit.Spades)))
                {
                    ClearAllSelections(true);

                    // The already-selected dealt card cannot be moved to the newly-selected target card pile.
                    if (cardAbove.Card != null)
                    {
                        AnnounceNoMove(cardAbove.Card);
                    }

                    return;
                }

                string cardRevealedAnnouncement = "";

                bool movedCard = false;

                // Should we move an Ace?
                if ((cardAbove.Card.Rank == 1) && (_targetPiles[targetPileIndex].Count == 0))
                {
                    // We will move the selected Ace over to its target pile.
                    var items = GetListSource(listAlreadySelected);
                    if (items == null)
                    {
                        return;
                    }

                    // Create a new Card object for use in the target pile.
                    Card newCard = new Card();
                    newCard.Rank = cardAbove.Card.Rank;
                    newCard.Suit = cardAbove.Card.Suit;

                    _targetPiles[targetPileIndex].Add(newCard);

                    // Now remove the source card from the Dealt Pile list.
                    var vm = this.BindingContext as DealtCardViewModel;
                    if ((vm != null) && (vm.DealtCards != null))
                    {
                        var listArray = vm.DealtCards[listAlreadySelectedIndex];

                        var previousItemCount = listArray.Count;

                        var removedItem = listArray[previousItemCount - 1];
                        RemoveDealtCardFromDealtCardCollection(items, removedItem);

                        var updatedItemCount = previousItemCount - 1;

                        DealtCard? cardRevealed = null;

                        // Is the dealt card pile now empty?
                        if (updatedItemCount == 0)
                        {
                            AddEmptyCardToCollectionView(items, listAlreadySelectedIndex);
                        }
                        else
                        {
                            // The source list is not empty. We will reveal the face-down card in the list
                            // where the card is moving from.
                            cardRevealed = (DealtCard)listArray[updatedItemCount - 1];

                            // Update the count of face-down cards in the pile in the bottom card in the pile.
                            UpdatePileFaceDownCount(listArray, cardRevealed);

                            listArray[updatedItemCount - 1].IsLastCardInPile = true;
                            listArray[updatedItemCount - 1].FaceDown = false;
                            listArray[updatedItemCount - 1].RefreshVisuals();
                        }

                        if (cardRevealed != null)
                        {
                            cardRevealed.CardState = CardState.FaceUp;

                            if (!string.IsNullOrEmpty(cardRevealed.AccessibleName))
                            {
                                cardRevealedAnnouncement = cardRevealed.AccessibleNameWithoutSelectionAndMofN;
                            }
                        }
                        else
                        {
                            cardRevealedAnnouncement = MainPage.MyGetString("EmptyCardPile");
                        }

                        listArray[listArray.Count - 1].RefreshVisuals();

                        movedCard = true;
                    }
                }
                else if (_targetPiles[targetPileIndex].Count > 0)
                {
                    // We're not moving an Ace, and the TargetPile already contains a card.
                    var cardBelow = (Card)_targetPiles[targetPileIndex][_targetPiles[targetPileIndex].Count - 1];

                    if ((cardBelow.Suit == cardAbove.Card.Suit) &&
                        (cardBelow.Rank == cardAbove.Card.Rank - 1))
                    {
                        // Create a new Card object for use in the target pile.
                        Card newCard = new Card();
                        newCard.Rank = cardAbove.Card.Rank;
                        newCard.Suit = cardAbove.Card.Suit;

                        _targetPiles[targetPileIndex].Add(newCard);

                        // Now remove the source card from the Dealt Pile list.
                        var vm = this.BindingContext as DealtCardViewModel;
                        if ((vm != null) && (vm.DealtCards != null))
                        {
                            var listArray = vm.DealtCards[listAlreadySelectedIndex];

                            var previousItemCount = listArray.Count;

                            // Remove the last item in the list.
                            var removedItem = listArray[previousItemCount - 1];
                            RemoveDealtCardFromDealtCardCollection(listArray, removedItem);

                            var updatedItemCount = previousItemCount - 1;

                            // Is the dealt card pile now empty?
                            if (updatedItemCount < 1)
                            {
                                AddEmptyCardToCollectionView(listArray, listAlreadySelectedIndex);

                                cardRevealedAnnouncement = MainPage.MyGetString("EmptyCardPile");
                            }
                            else
                            {
                                var cardRevealed = listArray[updatedItemCount - 1];

                                // Update the count of face-down cards in the pile in the bottom card in the pile.
                                UpdatePileFaceDownCount(listArray, cardRevealed);

                                cardRevealed.CardState = CardState.FaceUp;

                                listArray[updatedItemCount - 1].FaceDown = false;
                                listArray[updatedItemCount - 1].IsLastCardInPile = true;
                                listArray[updatedItemCount - 1].RefreshVisuals();

                                if (!string.IsNullOrEmpty(cardRevealed.AccessibleName))
                                {
                                    cardRevealedAnnouncement = cardRevealed.AccessibleNameWithoutSelectionAndMofN;
                                }

                            }

                            movedCard = true;
                        }
                    }
                }

                if (movedCard)
                {
                    // We know the target pile list isn't empty if we're here.
                    int count = _targetPiles[targetPileIndex].Count;
                    Card card = _targetPiles[targetPileIndex][count - 1];
                    targetPileCardButton.Card = card;

                    // Couldn't seem to get the binding to work for the suit colours, so set them explicitly here. 
                    SetCardSuitColours(targetPileCardButton);

                    // Have screen readers make a related announcement.
                    string announcement =
                        MainPage.MyGetString("Moved") + " " +
                        cardAbove.Card.GetCardAccessibleName();

                    if (currentGameType == SolitaireGameType.Klondike)
                    {
                        announcement += ", " +
                        revealedString + " " +
                        cardRevealedAnnouncement +
                        " " + inDealtCardPile + " " +
                        localizedNumbers[listAlreadySelectedIndex];
                    }

                    announcement += ".";

                    MakeDelayedScreenReaderAnnouncement(announcement, true);

                    RefreshDealtCardPileAccessibleNames(listAlreadySelected);
                }
                else
                {
                    // The already-selected dealt card cannot be moved to the newly-selected target card pile.
                    if (cardAbove.Card != null)
                    {
                        AnnounceNoMove(cardAbove.Card);
                    }
                }

                PlaySound(movedCard);

                SetCardButtonToggledSelectionState(targetPileCardButton, false);

                if (GameOver())
                {
                    ShowEndOfGameDialog(false);
                }
                else if (CheckForAutoComplete())
                {
                    AutoCompleteGameNow(true);
                }

                return;
            }
            else
            {
                // An attempt has been made to move an upturned card from a dealt card pile
                // to a target card pile, but the dealt card is not the last card in the pile.
                ClearAllSelections(false);

                PlaySound(false);
            }
        }

        private bool CheckForAutoComplete()
        {
            // Auto-complete is only available in specific circumstances in the Klondike game.
            if ((currentGameType != SolitaireGameType.Klondike) || 
                !OptionAutoCompleteGame || (OptionCardTurnCount != 1))
            {
                return false;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return false;
            }

            var autoComplete = true;

            // Are all cards in the dealt card piles face up now?
            for (int i = 0; i < GetCardPileCount(); i++)
            {
                if (vm.DealtCards[i].Count > 0)
                {
                    // If the bottom card is a face-up card, don't auto-complete.
                    if ((vm.DealtCards[i][0].CardState != CardState.KingPlaceHolder) &&
                        vm.DealtCards[i][0].FaceDown)
                    {
                        // This card is not face up, so don't autocomplete.
                        autoComplete = false;

                        break;
                    }
                }
            }

            Debug.WriteLine("CheckForAutoComplete: Auto-complete state " + autoComplete);

            return autoComplete;
        }

        private void AutoCompleteGameNow(bool showAutoCompleteMessage)
        {
            // Barker Todo: Move all the dealt cards and remaining cards 
            // up to the target card. For now, simply end the game.

            if (currentGameType != SolitaireGameType.Klondike)
            {
                return;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // First clear the remaining card piles.
            NextCardDeck.State = NextCardPileState.Empty;

            CardDeckUpturnedObscuredLower.Card = null;
            CardDeckUpturnedObscuredHigher.Card = null;
            CardDeckUpturned.Card = null;

            // Next clear all the dealt card piles.
            foreach (var dealtCardPile in vm.DealtCards)
            {
                dealtCardPile.Clear();
            }

            // Now set all the Target Card Pile to show Kings.

            // Barker Todo: Given that _targetPiles[] does not need updating here,
            // consider the role of those lists.

            Card newCardClubs = new Card();
            newCardClubs.Rank = 13;
            newCardClubs.Suit = Suit.Clubs;
            TargetPileC.Card = newCardClubs;

            Card newCardDiamonds = new Card();
            newCardDiamonds.Rank = 13;
            newCardDiamonds.Suit = Suit.Diamonds;
            TargetPileD.Card = newCardDiamonds;

            Card newCardHearts = new Card();
            newCardHearts.Rank = 13;
            newCardHearts.Suit = Suit.Hearts;
            TargetPileH.Card = newCardHearts;

            Card newCardSpades = new Card();
            newCardSpades.Rank = 13;
            newCardSpades.Suit = Suit.Spades;
            TargetPileS.Card = newCardSpades;

            if (showAutoCompleteMessage)
            {
                ShowEndOfGameDialog(true);
            }
        }

        private void RefreshDealtCardPileAccessibleNames(CollectionView collectionView)
        {
            var items = collectionView.ItemsSource;
            foreach (var item in items)
            {
                var dealtCard = item as DealtCard;
                if (dealtCard != null)
                {
                    dealtCard.RefreshAccessibleName();
                }
            }
        }

        private void RefreshAllDealtCardPileCardIsInAccessibleTree()
        {
            for (int i = 0; i < GetCardPileCount(); i++)
            {
                var collectionView = (CollectionView)CardPileGrid.FindByName("CardPile" + (i + 1));
                if (collectionView != null)
                {
                    var items = collectionView.ItemsSource;
                    foreach (var item in items)
                    {
                        var dealtCard = item as DealtCard;
                        if (dealtCard != null)
                        {
                            dealtCard.RefreshCardIsInAccessibleTree();
                        }
                    }
                }
            }
        }
    }
}
