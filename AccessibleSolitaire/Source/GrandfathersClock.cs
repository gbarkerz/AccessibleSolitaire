using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;
using System.Diagnostics;

namespace Sa11ytaire4All
{
    public partial class MainPage : ContentPage
    {
        private void InitialiseGrandfathersclockButtons()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            Debug.WriteLine("InitialiseGrandfathersclockButtons: START.");

            var clockButtons = TargetPilesClock.Children;

            if ((clockButtons == null) || (clockButtons.Count != 12))
            {
                Debug.WriteLine("InitialiseGrandfathersclockButtons: Unexpected clock buttons state.");

                return;
            }

            for (int i = 0; i < 12; i++)
            {
                var button = clockButtons[i] as CardButton;
                if (button != null)
                {
                    button.Margin = new Thickness(0, 0, 2, 0);
                    button.Padding = new Thickness(0);

                    if (vm.CardButtonsHeadingState && (i % 3 == 0))
                    {
                        button.SetHeadingState(true);
                    }

                    if ((vm.DealtCards != null) && (vm.DealtCards[8] != null) &&
                        (vm.DealtCards[8].Count == 12))
                    {
                        var dealtCard = vm.DealtCards[8][i];
                        if (dealtCard != null)
                        {
                            button.Card = dealtCard.Card;

                            button.StackDetails = dealtCard.StackDetails;

                            SetPyramidCardButtonBindingProperties(button);
                        }
                    }
                }
            }

            Debug.WriteLine("InitialiseGrandfathersclockButtons: Done.");

            ArrangeGrandfathersclockButtons();
        }

        private void ArrangeGrandfathersclockButtons()
        {
            Debug.WriteLine("ArrangeGrandfathersclockButtons: START.");

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            if ((InnerMainGrid.Height < 0) || (InnerMainGrid.Width < 0))
            {
                Debug.WriteLine("ArrangeGrandfathersclockButtons: InnerMainGrid not ready.");

                return;
            }

            Debug.WriteLine("ArrangeGrandfathersclockButtons: Arrange buttons now.");

            if (currentGameType == SolitaireGameType.Grandfathersclock)
            {
                TargetPilesClock.HeightRequest = (7 * InnerMainGrid.Height) / 15;

                var buttonWidthTotal = 0;

                if (vm.CardWidth > 0)
                {
                    buttonWidthTotal = (int)((vm.CardWidth / 3) + 4) * (vm.ShowScreenReaderAnnouncementButtons ? 2 : 1);
                }

                var clockCardWidth = (InnerMainGrid.Width - buttonWidthTotal) / 8;
                var clockCardHeight = (2 * 7 * InnerMainGrid.Height) / (1 * 8 * 15);

                // Account for the margins here.
                clockCardWidth -= 2;

                Debug.WriteLine("ArrangeGrandfathersclockButtons: clockCardWidth " + clockCardWidth +
                    ", clockCardHeight " + clockCardHeight);

                var clockCards = TargetPilesClock.Children;
                if ((clockCards != null) && (clockCards.Count == 12))
                {
                    for (var i = 0; i < clockCards.Count; i++)
                    {
                        var clockCard = TargetPilesClock[i] as CardButton;
                        if (clockCard != null)
                        {
                            clockCard.IsVisible = true;

                            clockCard.WidthRequest = clockCardWidth;

                            var isPortrait = (DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait);

                            clockCard.HeightRequest = clockCardHeight;

                            if ((i < 8) && (TargetPilesClock.RowDefinitions != null) &&
                                (TargetPilesClock.RowDefinitions[i] != null) &&
                                (clockCard.HeightRequest > 0))
                            {
                                Debug.WriteLine("ArrangeGrandfathersclockButtons: Set row " + i + " height " + 
                                    (clockCardHeight / 2));

                                TargetPilesClock.RowDefinitions[i] = new RowDefinition(
                                                                        new GridLength(clockCardHeight / 2,
                                                                            GridUnitType.Absolute));
                            }
                        }
                    }
                }
            }

            Debug.WriteLine("ArrangeGrandfathersclockButtons: Done.");
        }

        private Timer? timerDelayDealCardsToGrandfathersclockPostprocess;

        private void DelayedDealCardsToGrandfathersclockPostprocess(bool setDealtCardProperties)
        {
            if (timerDelayDealCardsToGrandfathersclockPostprocess == null)
            {
                timerDelayDealCardsToGrandfathersclockPostprocess = new Timer(
                                    new TimerCallback((s) => TimedDealCardsToGrandfathersclockPostprocess(setDealtCardProperties)),
                                        null,
                                        TimeSpan.FromMilliseconds(1000),
                                        TimeSpan.FromMilliseconds(Timeout.Infinite));
            }
        }

        private void TimedDealCardsToGrandfathersclockPostprocess(bool setDealtCardProperties)
        {
            timerDelayDealCardsToGrandfathersclockPostprocess?.Dispose();
            timerDelayDealCardsToGrandfathersclockPostprocess = null;

            // Always run this on the UI thread.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DealCardsToGrandfathersclockPostprocess(setDealtCardProperties);
            });
        }

        private void DealCardsToGrandfathersclockPostprocess(bool setDealtCardProperties)
        {
            Debug.WriteLine("DealCardsToGrandfathersclockPostprocess: START.");

            if (!IsGameCollectionViewBased())
            {
                return;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            if (vm.DealtCards[8].Count == 0)
            {
                return;
            }

            Debug.WriteLine("DealCardsToGrandfathersclockPostprocess: Continue.");

            // The first CollectionView does nothing in the Grandfather's Clock game.
            CardPile9.IsVisible = false;
            CardPile9.ItemsSource = null;

            var cardButtonsUI = TargetPilesClock.Children;
            if ((cardButtonsUI == null) || (cardButtonsUI.Count != 12))
            {
                return;
            }

            var cardUIIndex = 0;

            // The ninth pile contains the clock's initial cards.
            for (int i = 0; i < 12; ++i)
            {
                var dealtCard = vm.DealtCards[8][i];
                if (dealtCard == null)
                {
                    continue;
                }

                // All of these are zero-based.
                dealtCard.PyramidRow = 0;
                dealtCard.PyramidCardOriginalIndexInRow = i;
                dealtCard.PyramidCardCurrentIndexInRow = i;
                dealtCard.PyramidCardCurrentCountOfCardsOnRow = 12;

                dealtCard.FaceDown = false;
                dealtCard.Open = true;

                if (setDealtCardProperties && (dealtCard.Card != null))
                {
                    dealtCard.StackDetails = dealtCard.Card.Rank.ToString();
                }

                var cardUI = cardButtonsUI[cardUIIndex] as CardButton;
                if (cardUI != null)
                {
                    cardUI.Card = dealtCard.Card;

                    cardUI.IsVisible = true;
                    cardUI.IsFaceUp = true;

                    cardUI.RefreshAccessibleName();

                    cardUI.StackDetails = dealtCard.StackDetails;

                    SetPyramidCardButtonBindingProperties(cardUI);

                    cardUI.RefreshVisuals();

                    ++cardUIIndex;
                }
            }

            // Barker: Why does the mofn need refreshing here, but not with similar games?
            for (int i = 0; i < 8; i++)
            {
                var dealtCardPile = (CollectionView)CardPileGrid.FindByName("CardPile" + (i + 1));
                if ((dealtCardPile != null) && (dealtCardPile.ItemsSource != null))
                {
                    RefreshDealtCardPileAccessibleNames(dealtCardPile);
                }
            }

            Debug.WriteLine("DealCardsToGrandfathersclockPostprocess: Done.");
        }

        // Handle a click on one of the CardButtons in the Grandfather Clock.
        private void HandleGrandfathersclockCardClick(CardButton cardButton)
        {
            if ((cardButton == null) || (cardButton.Card == null) || 
                (currentGameType != SolitaireGameType.Grandfathersclock))
            {
                return;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // Get the already-selected card from the other list if there is one.
            CollectionView? listAlreadySelected;
            int listAlreadySelectedIndex;
            var cardAlreadySelected = GetSelectedDealtCard(null, // List to be ignored.
                                                 out listAlreadySelected,
                                                 out listAlreadySelectedIndex);

            if ((listAlreadySelected != null) &&
                (listAlreadySelectedIndex >= 0) && (listAlreadySelectedIndex < 8) &&
                (cardAlreadySelected != null) && (cardAlreadySelected.Card != null))
            {
                // Can the already selected card by moved to the clicked clock card?
                var moveOk = false;
                
                if (cardAlreadySelected.Card.Suit == cardButton.Card.Suit)
                {
                    moveOk = (cardAlreadySelected.Card.Rank == cardButton.Card.Rank + 1) ||
                              ((cardAlreadySelected.Card.Rank == 1) && (cardButton.Card.Rank == 13));
                }

                if (moveOk)
                {
                    // Keep track of all the cards added to this spot.
                    cardButton.StackDetails += " " + cardAlreadySelected.StackDetails;

                    CollectionView? list;
                    var dealtCard = FindDealtCardFromCard(cardButton.Card, false, out list);
                    if (dealtCard != null)
                    {
                        dealtCard.Card = cardAlreadySelected.Card;
                        dealtCard.StackDetails = cardButton.StackDetails;
                    }

                    // Move the dealt card to the clock.
                    cardButton.Card = cardAlreadySelected.Card;

                    // Now remove the card from the dealt card pile list.
                    var itemsSource = vm.DealtCards[listAlreadySelectedIndex];
                    var cardsCount = itemsSource.Count;

                    if (cardsCount > 0)
                    {
                        itemsSource.Remove(cardAlreadySelected);

                        --cardsCount;
                    }

                    string inDealtCardPile = MainPage.MyGetString("InDealtCardPile");
                    string revealedString = MainPage.MyGetString("Revealed");

                    string announcement =
                        MainPage.MyGetString("Moved") + " " +
                        cardButton.Card.GetCardAccessibleName() + " " +
                        MainPage.MyGetString("To") + " " +
                        MainPage.MyGetString("Clock");

                    announcement += ", " +
                        revealedString + " ";

                    // Check for the pile now being empty.
                    if (cardsCount > 0)
                    {
                        var newTopCard = itemsSource[cardsCount - 1];
                        if ((newTopCard != null) && (newTopCard.Card != null))
                        {
                            newTopCard.IsLastCardInPile = true;

                            announcement += newTopCard.Card.GetCardAccessibleName();
                        }
                    }
                    else
                    {
                        AddEmptyCardToCollectionView(itemsSource, listAlreadySelectedIndex);

                        announcement += MainPage.MyGetString("Empty");
                    }

                    announcement += " " + inDealtCardPile + " " + localizedNumbers[listAlreadySelectedIndex] + ". ";

                    MakeDelayedScreenReaderAnnouncement(announcement, true);

                    PlaySound(true);

                    cardButton.RefreshVisuals();
                }
                else
                {
                    PlaySound(false);

                    cardAlreadySelected.CardSelected = false;
                }

                if (GameOver())
                {
                    ShowEndOfGameDialog(false);
                }
            }
        }

        private bool IsGrandfathersclockCardCountUnexpected()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return true;
            }

            if (currentGameType != SolitaireGameType.Grandfathersclock)
            {
                return true;
            }
    
            var countUnexpected = false;

            var remainingDealtCardsCount = 0;

            for (var i = 0; i < 8; ++i)
            {
                var remainingCards = vm.DealtCards[i];
                if (remainingCards != null)
                {
                    for (var j = 0; j < remainingCards.Count; ++j)
                    {
                        var remainingCard = remainingCards[j];
                        if ((remainingCard != null) && (remainingCard.CardState != CardState.KingPlaceHolder))
                        {
                            ++remainingDealtCardsCount;
                        }
                    }
                }
            }

            var clockCardCount = 0;

            if ((vm.DealtCards[8] != null) && (vm.DealtCards[8].Count == 12))
            {
                for (var i = 0; i < 12; ++i)
                {
                    var spaceCount = 0;

                    var clockCard = vm.DealtCards[8][i];
                    if ((clockCard != null) && (clockCard.StackDetails != null))
                    {
                        foreach (var c in clockCard.StackDetails)
                        {
                            spaceCount += (c == ' ' ? 1 : 0);
                        }
                    }

                    clockCardCount += 1 + spaceCount;
                }
            }

            var totalCardCount = remainingDealtCardsCount + clockCardCount;

            Debug.WriteLine("IsGrandfathersclockCardCountUnexpected: totalCardCount " + totalCardCount);

            if (totalCardCount != 52)
            {
                countUnexpected = true;
            }

            return countUnexpected;
        }

        public bool IsGrandfathersclockCardPileFull(string? stackDetails)
        {
            if (string.IsNullOrEmpty(stackDetails))
            {
                return false;
            }

            var isFullPile = false;

            if ((stackDetails == "9 10 11 12") ||
                (stackDetails == "10 11 12 13 1") ||
                (stackDetails == "11 12 13 1 2") ||
                (stackDetails == "12 13 1 2 3") ||
                (stackDetails == "13 1 2 3 4") ||
                (stackDetails == "1 2 3 4") ||
                (stackDetails == "2 3 4 5") ||
                (stackDetails == "3 4 5 6") ||
                (stackDetails == "4 5 6 7") ||
                (stackDetails == "5 6 7 8") ||
                (stackDetails == "6 7 8 9") ||
                (stackDetails == "7 8 9 10") ||
                (stackDetails == "8 9 10 11"))
            {
                isFullPile = true;
            }

            return isFullPile;
        }
    }
}