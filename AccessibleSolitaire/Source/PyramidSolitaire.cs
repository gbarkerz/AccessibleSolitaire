using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
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

            PyramidOpenCardsAnnouncementButton.IsVisible = false;
        }

        public void LoadPyramidGame()
        {
            ChangeGameType(SolitaireGameType.Pyramid);

            // This button is only visible if the Show Screen Reader Buttons setting is on.
            PyramidOpenCardsAnnouncementButton.IsVisible = true;
        }

        private void ChangeGameType(SolitaireGameType targetGameType)
        {
            StopCelebratoryActions();

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

            CardPileGrid.IsVisible = isKlondike;

            ClearAllPiles();

            if (!LoadSession())
            {
                RestartGame(true /* screenReaderAnnouncement. */);
            }

            timerRefreshDealtCards = new Timer(
                new TimerCallback((s) => RefreshDealtCards()),
                    null,
                    TimeSpan.FromMilliseconds(200),
                    TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        private Timer? timerRefreshDealtCards;

        private void RefreshDealtCards()
        {
            timerRefreshDealtCards?.Dispose();
            timerRefreshDealtCards = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                RefreshAllCardVisuals();
            });
        }

        private void AddPyramidButtons()
        {
            CardButton button;

            var countOfCardsPerRow = 1;
            var countOfCardsOnCurrentRow = 0;

            var startColumn = 6;
            var currentColumn = 6;

            var vm = this.BindingContext as DealtCardViewModel;
            if (vm == null)
            {
                return;
            }

            var setSemanticHeading = true;

            for (int i = 0; i < 28; i++)
            {
                button = new CardButton();

                button.Margin = new Thickness(2, 0, 2, 0);

                button.IsToggled = false;

                if (Application.Current != null)
                {
                    button.BackgroundColor = (Application.Current.RequestedTheme != AppTheme.Dark ?
                                                Colors.White : Colors.Black);
                }

                CardPileGridPyramid.Children.Add(button);

                CardPileGridPyramid.SetRow(button, countOfCardsPerRow - 1);
                CardPileGridPyramid.SetRowSpan(button, 3);

                CardPileGridPyramid.SetColumn(button, currentColumn);
                CardPileGridPyramid.SetColumnSpan(button, 2);

                if (setSemanticHeading)
                {
                    var firstCardInRowIsHeading = vm.CardButtonsHeadingState;

                    button.SetHeadingState(firstCardInRowIsHeading);

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

        private void SetPyramidCardButtonBindingProperties(CardButton cardButton)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // Barker Todo: I've yet to get the binding to work on all the dynamically created cards.
            // So until I figure that out, set the values explicitly here, and require the game to 
            // be restarted if the related settings are changed.

            var image = (Image)cardButton.FindByName("TintedCardImage");
            if (image != null)
            {
                //button.BindingContext = vm;

                //button.SetBinding(CardButton.LongPressZoomDurationProperty, 
                //                    static (DealtCardViewModel vm) => vm.LongPressZoomDuration);

                //button.SetBinding(CardButton.SuitColoursClubsSwitchProperty,
                //                    static (DealtCardViewModel vm) => vm.SuitColoursClubs);

                cardButton.LongPressZoomDuration = vm.LongPressZoomDuration;

                var iconTintColorBehavior = new IconTintColorBehavior();
                iconTintColorBehavior.TintColor = Colors.Transparent;

                if (cardButton.Card != null)
                {
                    switch (cardButton.Card.Suit)
                    {
                        case Suit.Clubs:
                            iconTintColorBehavior.TintColor = vm.SuitColoursClubs;
                            break;
                        case Suit.Diamonds:
                            iconTintColorBehavior.TintColor = vm.SuitColoursDiamonds;
                            break;
                        case Suit.Hearts:
                            iconTintColorBehavior.TintColor = vm.SuitColoursHearts;
                            break;
                        case Suit.Spades:
                            iconTintColorBehavior.TintColor = vm.SuitColoursSpades;
                            break;
                        default:
                            iconTintColorBehavior.TintColor = vm.SuitColoursSpades;
                            break;
                    }
                }

                if (iconTintColorBehavior.TintColor != Colors.Transparent)
                {
                    image.Behaviors.Add(iconTintColorBehavior);
                }
            }
        }

        private bool DealPyramidCardsPostprocess(bool setDealtCardProperties)
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return false;
            }

            if (vm.DealtCards[0].Count == 0)
            {
                return false;
            }

            var cardButtonsUI = CardPileGridPyramid.Children;

            var countCardsPerRow = 1;

            var cardUIIndex = 0;

            var totalCountOfVisibleCardsInPyramid = 0;

            for (int i = 0; i < 7; ++i)
            {
                var currentVisibleIndexOfCardOnRow = 0;

                // How many cards are currently shows on this row?
                var countOfVisibleCardsOnRow = 0;

                try
                {
                    for (int j = 0; j < countCardsPerRow; ++j)
                    {
                        var dealtCard = vm.DealtCards[i][j];
                        if (dealtCard.Card != null)
                        {
                            ++countOfVisibleCardsOnRow;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("LoadSession: ex " + ex.Message);

                    return false;
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

                        SetPyramidCardButtonBindingProperties(cardUI);
                    }

                    ++cardUIIndex;
                }

                ++countCardsPerRow;
            }

            return true;
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

            var discardMessage = "";

            // Track whether we'll be removing the card from the pyramid.
            var removeCard = false;

            // First check whether the main upturned card is selected and totals 13.
            if (CardDeckUpturned.IsToggled && (CardDeckUpturned.Card != null))
            {
                // Remove the selected Upturned card if appropriate.
                string upturnedCardAccessibleName;
                removeCard = CanRemoveUpturnedCardAndPyramidCard(CardDeckUpturned, cardButtonClicked, out upturnedCardAccessibleName);
                if (removeCard)
                {
                    cardButtonClicked.IsToggled = false;

                    discardMessage = MainPage.MyGetString("Discarded");
                    discardMessage += " " + upturnedCardAccessibleName;

                    // There is now currently no Upturned card. Leave the state of the waste pile unchanged.
                    CardDeckUpturned.Card = null;
                }
                else
                {
                    // The clicked pryamid card does not total 13 with the already selected Upturned card.
                    PlaySound(false);
                }
            }

            // If we didn't remove the Upturned card, check the top of the Waste pile (ie the higher obscured card).
            if (!removeCard)
            {
                if (CardDeckUpturnedObscuredHigher.IsToggled && (CardDeckUpturnedObscuredHigher.Card != null))
                {
                    string obscuredHigherCardAccessibleName;
                    removeCard = CanRemoveUpturnedCardAndPyramidCard(CardDeckUpturnedObscuredHigher, cardButtonClicked,
                                                                        out obscuredHigherCardAccessibleName);
                    if (removeCard)
                    {
                        CardDeckUpturnedObscuredHigher.IsToggled = false;

                        discardMessage = MainPage.MyGetString("Discarded");
                        discardMessage += " " + obscuredHigherCardAccessibleName;

                        CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 1 ?
                                            _deckUpturned[_deckUpturned.Count - 2] : null);
                    }
                    else
                    {
                        // The clicked pryamid card does not total 13 with the already selected card on the Waste pile.
                        PlaySound(false);
                    }
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
                            discardMessage = MainPage.MyGetString("Discarded");
                            discardMessage += " " + dealtCardAlreadySelected.AccessibleNameWithoutSelectionAndMofN;

                            // Has a card now been revealed? 
                            SetOnTopStateFollowingMove(dealtCardAlreadySelected, false);

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
                else
                {
                    // Change the selection state of the target pile CardButton.
                    //SetCardButtonToggledSelectionState(cardButtonClicked, false);

                    cardButtonClicked.RefreshCardButtonMofN();

                    // No other card was selected, so we'll simply select the pyramid card.
                    string? announcement = cardButtonClicked.CardPileAccessibleNameWithoutMofN;
                    MakeDelayedScreenReaderAnnouncement(announcement, false);
                }
            }

            // Should we remove the clicked pyramid card?
            if (removeCard)
            {
                CollectionView? list;
                var dealtCard = FindDealtCardFromCard(cardButtonClicked.Card, false, out list);
                if (dealtCard != null)
                {
                    if (string.IsNullOrEmpty(discardMessage))
                    {
                        discardMessage = MainPage.MyGetString("Discarded");
                    }
                    else
                    {
                        discardMessage += " " + MainPage.MyGetString("And") + " ";
                    }

                    discardMessage += " " + dealtCard.AccessibleNameWithoutSelectionAndMofN + ". ";

                    // Has a card now been revealed? 
                    SetOnTopStateFollowingMove(dealtCard, true);

                    RefreshCardButtonMofNInRow(cardButtonClicked);

                    vm.DealtCards[dealtCard.PyramidRow][dealtCard.PyramidCardOriginalIndexInRow].Card = null;

                    cardButtonClicked.IsVisible = false;
                    cardButtonClicked.IsToggled = false;

                    PlaySound(true);
                }
            }
            else
            {
                // The clicked card was not removed. If another pyramid card was already selected, deselect it.
                if (cardAlreadySelected != null)
                {
                    cardButtonClicked.IsToggled = false;

                    cardAlreadySelected.IsToggled = false;

                    PlaySound(false);
                }
            }

            if (!string.IsNullOrEmpty(discardMessage))
            {
                // Barker Todo: Figure out how to announce the results of the operation without
                // the announcement conflicting with the default VoiceOver announcement relating
                // to focus moving.
                //MakeDelayedScreenReaderAnnouncementWithDelayTime(discardMessage, true, 2000);

                // Barker Todo: Still announce the available moves if necessary.
                if (automaticallyAnnounceMoves)
                {
                    var availableMoveAnnouncement = AnnounceAvailableMoves(false);
                    if (!string.IsNullOrEmpty(availableMoveAnnouncement))
                    {
                        MakeDelayedScreenReaderAnnouncementWithDelayTime(availableMoveAnnouncement, false, 3000);
                    }
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
                var discardMessage = "";

                if (cardDeckUpturned.Card.Rank == 13)
                {
                    discardMessage = MainPage.MyGetString("Discarded") + " " +
                                        cardDeckUpturned.Card.GetCardAccessibleName();

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
                            string upturnedCardAccessibleName;
                            var removeCard = CanRemoveUpturnedCardAndPyramidCard(cardDeckUpturned, cardAlreadySelected,
                                                                                    out upturnedCardAccessibleName);
                            if (removeCard)
                            {
                                CollectionView? list;
                                var dealtCardAlreadySelected = FindDealtCardFromCard(cardAlreadySelected.Card, false, out list);
                                if (dealtCardAlreadySelected != null)
                                {
                                    cardAlreadySelected.IsToggled = false;

                                    discardMessage = MainPage.MyGetString("Discarded") + " " +
                                                        upturnedCardAccessibleName + " " +
                                                        MainPage.MyGetString("And") + " " +
                                                        cardAlreadySelected.CardPileAccessibleNameWithoutMofN;

                                    // Has a card now been revealed? 
                                    SetOnTopStateFollowingMove(dealtCardAlreadySelected, false);

                                    RefreshCardButtonMofNInRow(cardAlreadySelected);

                                    vm.DealtCards[dealtCardAlreadySelected.PyramidRow]
                                        [dealtCardAlreadySelected.PyramidCardOriginalIndexInRow].Card = null;

                                    cardAlreadySelected.IsVisible = false;

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
                                // The clicked upturned card does not total 13 with the already selected pyramid card.
                                PlaySound(false);
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

                if (!string.IsNullOrEmpty(discardMessage))
                {
                    // Barker Todo: Figure out how to announce the results of the operation without
                    // the announcement conflicting with the default VoiceOver announcement relating
                    // to focus moving.
                    //MakeDelayedScreenReaderAnnouncement(discardMessage, true);

                    // Barker Todo: Still announce the available moves if necessary.
                    if (automaticallyAnnounceMoves)
                    {
                        var availableMoveAnnouncement = AnnounceAvailableMoves(false);
                        if (!string.IsNullOrEmpty(availableMoveAnnouncement))
                        {
                            MakeDelayedScreenReaderAnnouncementWithDelayTime(availableMoveAnnouncement, false, 3000);
                        }
                    }

                    PlaySound(true);
                }
            }
        }

        // A pyramid card has been clicked. Remove the Upturned card if appropriate.
        private bool CanRemoveUpturnedCardAndPyramidCard(CardButton upturnedCard, CardButton cardButton, 
                                                            out string upturnedCardAccessibleName)
        {
            upturnedCardAccessibleName = "";

            if ((upturnedCard.Card == null) || (cardButton.Card == null))
            {
                return false;
            }

            bool canRemoveUpturnedCardAndPyramidCard = false;

            if (upturnedCard.Card.Rank + cardButton.Card.Rank == 13)
            {
                upturnedCard.IsToggled = false;

                upturnedCardAccessibleName = upturnedCard.CardPileAccessibleName;

                RemoveCardButtonCard(_deckUpturned, upturnedCard.Card);

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
        }

        // Check if both the Upturned card and HigherObscured card should be removed.
        private bool MoveBothUpturnedCards(CardButton cardButtonClicked)
        {
            var continueCheckForMove = true;

            if ((CardDeckUpturned.Card == null) || (CardDeckUpturnedObscuredHigher.Card == null))
            {
                return continueCheckForMove;
            }

            var checkForMove = ((cardButtonClicked == CardDeckUpturned) && CardDeckUpturnedObscuredHigher.IsToggled) ||
                               ((cardButtonClicked == CardDeckUpturnedObscuredHigher) && CardDeckUpturned.IsToggled);

            if (checkForMove)
            {
                continueCheckForMove = false;

                if (CardDeckUpturned.Card.Rank + CardDeckUpturnedObscuredHigher.Card.Rank == 13)
                {
                    RemoveCardButtonCard(_deckUpturned, CardDeckUpturned.Card);
                    RemoveCardButtonCard(_deckUpturned, CardDeckUpturnedObscuredHigher.Card);

                    CardDeckUpturned.Card = null;

                    CardDeckUpturnedObscuredHigher.Card = (_deckUpturned.Count > 0 ?
                                        _deckUpturned[_deckUpturned.Count - 1] : null);

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

                    PlaySound(true);
                }
                else
                {
                    CardDeckUpturnedObscuredHigher.IsToggled = false;
                    CardDeckUpturned.IsToggled = false;

                    PlaySound(false);
                }
            }

            return continueCheckForMove;
        }

        private void RefreshAccessibleCardCountInRow(CardButton cardButton, int pyramidRow)
        {
            Debug.WriteLine("RefreshAccessibleCardCountInRow: Refresh cards in row " + pyramidRow);

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

        private CardButton? GetCardButtonFromPyramidDealtCard(DealtCard dealtCard, out int cardButtonPyramidIndex)
        {
            cardButtonPyramidIndex = 0;

            CardButton? cardButtonFound = null;

            if ((dealtCard != null) && (dealtCard.Card != null))
            {
                var cardButtons = CardPileGridPyramid.Children;
                if ((cardButtons != null) && (cardButtons.Count > 0))
                { 
                    for (int i = 0; i < cardButtons.Count; ++i)
                    {
                        var cardButton = (cardButtons[i] as CardButton);
                        if ((cardButton != null) && (cardButton.Card == dealtCard.Card))
                        {
                            cardButtonFound = cardButton;

                            cardButtonPyramidIndex = i;

                            break;
                        }
                    }
                }
            }

            return cardButtonFound;
        }

        // **************************************************************************************************
        //
        // VERY IMPORTANT!
        //
        // SetOnTopStateFollowingMove() is being called because a card in the pyramid has been discarded.
        // If the card has focus, then focus will move away from the card, and by default, move to wherever
        // MAUI wants to put it. Sometimes the target seems unintuitive, so we should explicitly move focus
        // to somewhere more predictable. Note that in test on my iPad, keyboard focus is not helpful here,
        // and attempting to set keyboard focus and have it drag VoiceOver focus with it, doesn't work.
        // (ie Calls to Focus() fail, and VoiceOver seems unaffected by the attempt.) As such, explicitly
        // call SemanticSetFocus() to make sure VoiceOver ends up somewhere predictable and helpful.

        // HOWEVER: There is some undeterminable delay between the call to SemanticSetFocus() and VoiceOver
        // moving to the target element and making the related announcement. This means that if a custom
        // announcement related to the cards being discarded is still in progress, that announcement gets
        // stopped. No good away to avoid this problem has yet been found. There are no known events 
        // relating to VoiceOver moving to the target element, or relating to screen reader audio finishing.
        // As such, simply at a longer-than-usual delay before beginning the custom announcement relating
        // to cards being discarded. This is very unfortunate, as at best it results in a long delay before
        // learning about the results of discard action (and even then, the announcement relating to the
        // target element will have started and truncated), and at worst the delay in starting the 
        // discard-related announcement is insufficient and that announcement still gets truncated.
        //
        // **************************************************************************************************

        private void SetOnTopStateFollowingMove(DealtCard dealtCard, bool moveFocus)
        {
            //Debug.WriteLine("SetOnTopStateFollowingMove: " + dealtCard.AccessibleNameWithoutSelectionAndMofN);

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // Consider moving focus for the screen reader to follow if necessary.
            if (moveFocus && (dealtCard.PyramidRow > 0))
            {
                var cardButtonsUI = CardPileGridPyramid.Children;
                if (cardButtonsUI.Count > 0)
                {
                    CardButton? previousPyramidCard = null;
                    for (int i = 0; i < cardButtonsUI.Count; ++i)
                    {
                        var pyramidCard = (cardButtonsUI[i] as CardButton);
                        if ((pyramidCard != null) && pyramidCard.IsVisible)
                        {
                            if ((pyramidCard.Card != null) && (pyramidCard.Card == dealtCard.Card))
                            {
                                break;
                            }

                            previousPyramidCard = pyramidCard;
                        }
                    }

                    if (previousPyramidCard != null)
                    {
                        Debug.WriteLine("SetOnTopStateFollowingMove: Set semantic focus to " + 
                            previousPyramidCard.CardPileAccessibleName);

#if WINDOWS
                        previousPyramidCard.Focus();
#else
                        previousPyramidCard.SetSemanticFocus();
#endif
                    }
                }
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

            // While we're here, check if the card being removed is a heading.
            if (vm.CardButtonsHeadingState)
            { 
                int cardButtonPyramidIndex;
                var cardButton = GetCardButtonFromPyramidDealtCard(dealtCard, out cardButtonPyramidIndex);
                if (cardButton != null)
                {
                    var innerButton = (Button)cardButton.FindByName("InnerButton");
                    if (innerButton != null)
                    {
                        var headingLevel = SemanticProperties.GetHeadingLevel(innerButton);
                        if (headingLevel != SemanticHeadingLevel.None)
                        {
                            Debug.WriteLine("Found existing heading: " + cardButton.CardPileAccessibleName);

                            var cardButtons = CardPileGridPyramid.Children;
                            if (cardButtons != null)
                            {
                                if ((cardButtonPyramidIndex >= 0) &&
                                    (cardButtonPyramidIndex < cardButtons.Count - 1))
                                {
                                    for (int i = cardButtonPyramidIndex + 1; i < cardButtons.Count; ++i)
                                    {
                                        var nextCardButton = cardButtons[i] as CardButton;
                                        if ((nextCardButton != null) && nextCardButton.IsVisible)
                                        {
                                            Debug.WriteLine("Set as Heading: " + nextCardButton.CardPileAccessibleName);

                                            nextCardButton.SetHeadingState(true);

                                            break;
                                        }
                                    }
                                }
                            }
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