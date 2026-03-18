using Sa11ytaire4All.Source;
using Sa11ytaire4All.ViewModels;
using Sa11ytaire4All.Views;
using System.Diagnostics;

namespace Sa11ytaire4All
{
    public partial class MainPage : ContentPage
    {
        private void AddGrandfathersclockButtons()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // First add all the CardButtons for the clock.
            TargetPiles.Children.Clear();

            for (int i = 0; i < 12; i++)
            {
                var button = new CardButton();

                button.Margin = new Thickness(2, 0, 2, 0);
                button.IsToggled = false;

                TargetPiles.Children.Add(button);
            }

            ArrangeGrandfathersclockButtons();
        }

        private void ArrangeGrandfathersclockButtons()
        {
            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return;
            }

            // Assume available height is less than the available width.
            var availableHeight = 2 * InnerMainGrid.Height / 3;
            if (availableHeight < 0)
            {
                return;
            }

            var radius = (availableHeight - vm.CardHeight) / 2;

            var foundationCards = TargetPiles.Children; 
            if ((foundationCards == null) || (foundationCards.Count != 12))
            {
                return;
            }

            for (int i = 0; i < 12; i++)
            {
                var button = foundationCards[i] as CardButton;
                if (button == null)
                {
                    break;
                }

                // Barker: Check this scaling is acceptable on different screen sizes.
                var sizeFactor = 0.75;

                button.WidthRequest = sizeFactor * vm.CardWidth;
                button.HeightRequest = sizeFactor * vm.CardHeight;

                var angle = UnitConverters.DegreesToRadians(30 * i);

                var stretchFactor = sizeFactor;

                if ((i >= 0) && (i < 3))
                {
                    stretchFactor *= (i % 3);
                }
                else if ((i >= 3) && (i < 6))
                {
                    stretchFactor *= (3 - (i % 3));
                }
                else if ((i >= 6) && (i < 9))
                {
                    stretchFactor *= -(i % 3);
                }
                else
                {
                    stretchFactor *= -(3 - (i % 3));
                }

                var xTranslate = stretchFactor * vm.CardWidth;

                //var xTranslate = radius * Math.Sin(angle);

                // Move a little away from the top border.
                var yTranslate = ((availableHeight - vm.CardHeight + 16) / 2) - (radius * Math.Cos(angle));

                button.TranslateToAsync(xTranslate, yTranslate);

                //button.RotateToAsync(30 * i);
            }
        }

        private bool DealCardsToGrandfathersclockPostprocess()
        {
            if (Application.Current == null)
            {
                return false;
            }

            if (!IsGameCollectionViewBased())
            {
                return false;
            }

            var vm = this.BindingContext as DealtCardViewModel;
            if ((vm == null) || (vm.DealtCards == null))
            {
                return false;
            }

            if (vm.DealtCards[8].Count == 0)
            {
                return false;
            }

            // The first CollectionView does nothing in the Grandfather's Clock game.
            CardPile9.IsVisible = false;
            CardPile9.ItemsSource = null;

            var cardButtonsUI = TargetPiles.Children;
            if ((cardButtonsUI == null) || (cardButtonsUI.Count != 12))
            {
                return false;
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

                var cardUI = cardButtonsUI[cardUIIndex] as CardButton;
                if (cardUI != null)
                {
                    cardUI.IsVisible = true;
                    cardUI.Card = dealtCard.Card;
                    cardUI.IsFaceUp = true;

                    cardUI.RefreshAccessibleName();

                    SetPyramidCardButtonBindingProperties(cardUI);

                    ++cardUIIndex;
                }
            }

            return true;
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

            var discardMessage = "";

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
                    CollectionView? list;
                    var dealtCard = FindDealtCardFromCard(cardButton.Card, false, out list);
                    if (dealtCard != null)
                    {
                        dealtCard.Card = cardAlreadySelected.Card;
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

                    // Check for the pile now being empty.
                    if (cardsCount > 0)
                    {
                        var newTopCard = itemsSource[cardsCount - 1];
                        if (newTopCard != null)
                        {
                            newTopCard.IsLastCardInPile = true;
                        }
                    }
                    else
                    {
                        AddEmptyCardToCollectionView(itemsSource, listAlreadySelectedIndex);
                    }
                }
                else
                {
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
                    var clockCard = vm.DealtCards[8][i];
                    if ((clockCard != null) && (clockCard.Card != null))
                    {
                        var difference = 0;

                        if ((i > 0) && (i < 5))
                        {
                            var topCardRank = clockCard.Card.Rank;
                            if (topCardRank < 5)
                            {
                                topCardRank += 13;
                            }

                            difference = topCardRank - i - 9;
                        }
                        else
                        {
                            difference = (clockCard.Card.Rank + 3 - i) % 12;
                        }

                        clockCardCount += 1 + difference;
                    }
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
    }
}